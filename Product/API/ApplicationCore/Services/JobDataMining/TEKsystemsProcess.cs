﻿using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Services.JobDataMining
{
    public class TEKsystemsProcess : BaseProcess, IJobDataMining
    {
        public TEKsystemsProcess(JobSite jobSite, ILogger logger, Guid companyGuid) : base(jobSite, logger, companyGuid) { }

        public List<JobPage> DiscoverJobPages(List<JobPage> existingJobPages)
        {
            // populate this collection with the results of the job discovery operation
            ConcurrentBag<JobPage> jobPages = new ConcurrentBag<JobPage>();

            // diagnostics - remove this once we have tuned the process
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            string response;
            using (var client = new HttpClient())
            {
                // call the api to retrieve a total number of job results
                var request = new HttpRequestMessage()
                {
                    RequestUri = _jobSite.Uri,
                    Method = HttpMethod.Get
                };
                var result = client.SendAsync(request).Result;
                response = result.Content.ReadAsStringAsync().Result;
            }
            dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(response);
            int jobCount = Convert.ToInt32(jsonData.num_found);

            /* haven't been able to determine a way to change the size of the 'results' collection 
             * using query string params, it seems to always be 10. until we can figure out a way 
             * to retrieve all job postings at once, divide the number of jobs by 10 (rounding up) 
             * and call this endpoint that many times to ensure we get all jobs 
             */
            int timesToRequestResultsPage = Convert.ToInt32(Math.Ceiling((double)jobCount / 10));

            /* run the paged requests in parallel - tested with a variety of MAXDOP settings and 50 was the sweet spot locally; 
             * may need to adjust this once it is running in azure - leaving it up to the executing process to determine how many 
             * threads to use for now. use maxdop = 1 for debugging.
             */
            var maxdop = new ParallelOptions { MaxDegreeOfParallelism = -1 };
            int counter = 0;
            Parallel.For(counter, timesToRequestResultsPage, maxdop, i =>
           {
               string jobData;
               using (var client = new HttpClient())
               {
                   // call the api to retrieve a list of results incrementing the page number each time
                   int progress = Interlocked.Increment(ref counter);
                   UriBuilder builder = new UriBuilder(_jobSite.Uri);
                   builder.Query += "&page=" + progress.ToString();
                   var request = new HttpRequestMessage()
                   {
                       RequestUri = builder.Uri,
                       Method = HttpMethod.Get
                   };
                   var result = client.SendAsync(request).Result;
                   jobData = result.Content.ReadAsStringAsync().Result;
               }
               dynamic jsonResults = JsonConvert.DeserializeObject<dynamic>(jobData);

               // keeping this loop serial rather than parallel intentionally (nesting parallel loops can quickly cause performance issues)
               foreach (var job in jsonResults.results)
               {
                   int jobPageStatusId = 1; // pending
                   string rawHtml;
                   Uri jobDetailUri = null;
                   try
                   {
                       // retrieve the latest job page data
                        jobDetailUri = new Uri(_jobSite.Uri.GetLeftPart(System.UriPartial.Authority) + job.job_details_url);
                       using (var client = new HttpClient())
                       {
                           var request = new HttpRequestMessage()
                           {
                               RequestUri = jobDetailUri,
                               Method = HttpMethod.Get
                           };
                           var result = client.SendAsync(request).Result;
                           rawHtml = result.Content.ReadAsStringAsync().Result;
                       }
                       HtmlDocument jobHtml = new HtmlDocument();
                       jobHtml.LoadHtml(rawHtml);

                       // does the html contain an error message indicating the job does not exist?
                       bool isJobExists = jobHtml.DocumentNode.SelectSingleNode("//results-main[@error-message=\"The job you have requested cannot be found. Please see our complete list of jobs below.\"]") == null ? true : false;
                       if (!isJobExists)
                       {
                           jobPageStatusId = 4; // delete
                       }
                       else
                       {
                           // append additional data that is not present in search results for the page, status already marked as new
                           var scripNodetWithJson = jobHtml.DocumentNode.SelectSingleNode("(//script[@type='application/ld+json'])[1]");
                           if (scripNodetWithJson != null)
                           {
                               var jobDataJson = JsonConvert.DeserializeObject<dynamic>(scripNodetWithJson.InnerHtml.ToString());
                               job.responsibilities = jobDataJson.responsibilities.Value;
                           }
                       }


                       // get the related JobPostingId (if one exists)
                       string jobId = job.id;
                       var existingJobPage = existingJobPages.Where(jp => jp.Uri.ToString() == jobDetailUri.ToString() && jp.UniqueIdentifier == jobId).FirstOrDefault();
                       if (existingJobPage != null)
                       {
                           // add the updated job page to the collection
                           if (existingJobPage.RawData != job.ToString())
                           {
                               existingJobPage.JobPageStatusId = jobPageStatusId;
                               existingJobPage.RawData = job.ToString();
                               existingJobPage.ModifyDate = DateTime.UtcNow;
                               existingJobPage.ModifyGuid = Guid.Empty;
                               jobPages.Add(existingJobPage);
                           }
                       }
                       else
                       {
                           // add the new job page to the collection
                           jobPages.Add(new JobPage()
                           {
                               CreateDate = DateTime.UtcNow,
                               CreateGuid = Guid.Empty,
                               IsDeleted = 0,
                               JobPageGuid = Guid.NewGuid(),
                               JobPageStatusId = jobPageStatusId,
                               RawData = job.ToString(),
                               UniqueIdentifier = job.id.ToString(),
                               Uri = jobDetailUri,
                               JobSiteId = _jobSite.JobSiteId
                           });
                       }
                   }
                   catch (Exception e)
                   {
                       jobPageStatusId = 3; // error
                       _syslog.Log(LogLevel.Error, $"***** TEKsystemProcess.DiscoverJobPages encountered an exception: {e.Message}");
                   }
               }
           });

            /* deal with duplicate job postings (or job postings that are similar enough to be considered duplicates). examples:
             * - two job listings that have the same url and id but in the raw data the "applications" property is different (id: J3Q20V76L8YK2XBR6S8)
             * - two job listings that have the same id but different urls. when looking at the website, each lists a different canonical url. 
             *      we don't want to make the same mistake as this will hurt us from an SEO perspective.
             * 
             * the goal of the code below is to eliminate these duplicates in a repeatable manner before they make it to our db (dbo.JobPage). the 
             * method used is secondary to the goal of it being repeatable (in terms of importance). what we want to avoid is the following: two jobs 
             * are essentially identical; job A and job B. if yesterday the job data mining process chose job A and ignored job B, and tomorrow it 
             * chooses job B instead of job A, the end result could be inconsistent data in our scraped job and job applications. in addition, the 
             * audit trail of what we did could be quite confusing.
             */
            var uniqueNewJobs = (from jp in jobPages
                                 group jp by jp.UniqueIdentifier into g
                                 select g.OrderBy(a => a, new CompareByUri()).First()).ToList();

            /* mark anything in pending status that is not in the above list as deleted. to accomplish this, do the following:
             * - compare unique new jobs to the list of existing job pages (active - new, processed)
             * - existingNewJobs.Except(uniqueNewJobs) -> add these to the output and mark as deletes
             */
            var unreferencedJobs = existingJobPages.AsQueryable().Except(uniqueNewJobs, new EqualityComparerByUri()).ToList();
            var jobsToDelete = unreferencedJobs.Select(jp => { jp.JobPageStatusId = 4; return jp; }).ToList();

            // combine new/modified jobs and unreferenced jobs which should be deleted
            List<JobPage> updatedJobPages = new List<JobPage>();
            updatedJobPages.AddRange(uniqueNewJobs);
            updatedJobPages.AddRange(jobsToDelete);

            // diagnostics - remove this once we have tuned the process
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;

            return updatedJobPages;
        }

        public JobPostingDto ProcessJobPage(JobPage jobPage)
        {
            JobPostingDto jobPostingDto = new JobPostingDto();
            try
            {
                // set everything we can without relying on the raw data from the job page
                jobPostingDto.CreateGuid = Guid.Empty;
                jobPostingDto.IsDeleted = 0;
                jobPostingDto.IsAgencyJobPosting = true;
                jobPostingDto.ThirdPartyApplicationUrl = jobPage.Uri.ToString();
                jobPostingDto.ThirdPartyApply = true;
                jobPostingDto.JobStatus = (int)JobPostingStatus.Active;
                jobPostingDto.Company = new CompanyDto() { CompanyGuid = _companyGuid };

                // everything else relies upon valid raw data
                if (!string.IsNullOrWhiteSpace(jobPage.RawData))
                {
                    var jobData = JsonConvert.DeserializeObject<dynamic>(jobPage.RawData);
                    jobPostingDto.Description = jobData.responsibilities;
                    jobPostingDto.City = jobData.city;
                    DateTime datePosted;
                    if (DateTime.TryParse(jobData.date_posted.ToString(), out datePosted))
                        jobPostingDto.CreateDate = datePosted;
                    else
                        jobPostingDto.CreateDate = DateTime.UtcNow;
                    jobPostingDto.Title = jobData.job_title;
                    jobPostingDto.Province = jobData.admin_area_1;
                    string recruiterName = jobData.discrete_field_3;
                    string recruiterFirstName = null, recruiterLastName = null;
                    if (!string.IsNullOrWhiteSpace(recruiterName))
                    {
                        string[] tmp = recruiterName.Split(' ');
                        if (tmp.Length == 2)
                        {
                            recruiterFirstName = tmp[0];
                            recruiterLastName = tmp[1];
                        }
                    }
                    jobPostingDto.Recruiter = new RecruiterDto()
                    {
                        Email = jobData.discrete_field_4,
                        FirstName = recruiterFirstName,
                        LastName = recruiterLastName
                    };
                }

                return jobPostingDto;
            }
            catch (Exception e)
            {
                _syslog.Log(LogLevel.Error, $"***** TEKsystemProcess.ProcessJobPage encountered an exception: {e.Message}");
                return null;
            }
        }

        public class EqualityComparerByUri : IEqualityComparer<JobPage>
        {
            public bool Equals(JobPage x, JobPage y)
            {
                if (Object.ReferenceEquals(x, y))
                    return true;
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;
                return x.Uri == y.Uri;
            }

            public int GetHashCode(JobPage jobPage)
            {
                if (Object.ReferenceEquals(jobPage, null))
                    return 0;
                return jobPage.Uri == null ? 0 : jobPage.Uri.GetHashCode();
            }
        }

        public class CompareByUri : IComparer<JobPage>
        {
            public int Compare(JobPage x, JobPage y)
            {
                return string.Compare(x.Uri.ToString(), y.Uri.ToString());
            }
        }
    }
}
