﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using Google.Protobuf.WellKnownTypes;
using Google.Apis.CloudTalentSolution.v3;
using UpDiddyLib.Helpers;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Factory;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Services;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi.Helpers.Job
{
    /// <summary>
    /// Helper class for dealing with mapping cc jobpostings to google talent cloud data types
    /// </summary>
    public class JobHelper
    {


        #region Cloud talent job -> CC job helpers

        public static JobViewDto CreateJobView( CloudTalentSolution.MatchingJob matchingJob)
        {
            JobViewDto  rVal = new JobViewDto();
            rVal.JobSummary = matchingJob.JobSummary;
            rVal.JobTitleSnippet = matchingJob.SearchTextSnippet;
            rVal.SearchTextSnippet = matchingJob.SearchTextSnippet;
            Guid postingGuid = Guid.Empty;
            Guid.TryParse(matchingJob.Job.RequisitionId, out postingGuid);
            rVal.JobPostingGuid = postingGuid;
            rVal.PostingExpirationDateUTC = DateTime.Parse(matchingJob.Job.PostingExpireTime.ToString());
            rVal.CloudTalentUri = matchingJob.Job.Name;
            rVal.CompanyName = matchingJob.Job.CompanyName;
            rVal.Title = matchingJob.Job.Title;
            rVal.Description = matchingJob.Job.Description;
            rVal.PostingDateUTC =  DateTime.Parse(matchingJob.Job.PostingPublishTime.ToString() );
            // map location that was indexed into google -- do not use a foreach loop since it's sloooooow (might be string concat0
            if (matchingJob.Job.Addresses != null && matchingJob.Job.Addresses.Count > 0 )
                rVal.Location = matchingJob.Job.Addresses[0];
                  
            return rVal;
        }



        /// <summary>
        /// Map cloud talent job results to cc jobsearch results 
        /// </summary>
        /// <param name="searchJobsResponse"></param>
        /// <returns></returns>
        /// TODO JAB finish mapping 
        public static JobSearchResultDto MapSearchResults(ILogger syslog, IMapper mapper, IConfiguration configuration, CloudTalentSolution.SearchJobsResponse searchJobsResponse, JobQueryDto jobQuery)
        {

            JobSearchResultDto rVal = new JobSearchResultDto();
            // handle case of no jobs found 
            if ( searchJobsResponse.MatchingJobs == null )
            {
                rVal.JobCount = 0;
                return rVal;
            }

            rVal.JobCount = searchJobsResponse.MatchingJobs.Count;
            rVal.TotalHits = searchJobsResponse.TotalSize.Value;

            foreach (CloudTalentSolution.MatchingJob j in searchJobsResponse.MatchingJobs)
            {
                try
                {
                    // Automapper is too slow so do the mapping the old fashion way                    
                    JobViewDto jv = CreateJobView(j);                                      
                    // Map custom attributes to job view
                    JobHelper.MapCustomJobPostingAttributes(j, jv);
                    rVal.Jobs.Add(jv);
                }
                catch (Exception e)
                {
                    syslog.LogError(e, "JobPostingFactory.MapSearchResults Error mapping job", e, j);
                }
            }
            // add facets to the search results 
            rVal.Facets = JobHelper.MapFacets(configuration,jobQuery, searchJobsResponse);
            return rVal;
        }

        static public List<JobQueryFacetDto> MapFacets(IConfiguration config, JobQueryDto jobQuery, CloudTalentSolution.SearchJobsResponse searchJobsResponse)
        {
            List<JobQueryFacetDto> rVal = new List<JobQueryFacetDto>();

            string TopLevelDomain = config["CloudTalent:TopLevelDomain"].ToString();
            string IndustryUrl = JobUrlHelper.GetDefaultIndustryUrl(jobQuery);
            string LocationtUrl = JobUrlHelper.GetDefaultLocationUrl(jobQuery);

            // Map simple histogram results 
            foreach (CloudTalentSolution.HistogramResult hr in searchJobsResponse.HistogramResults.SimpleHistogramResults)
            {
                JobQueryFacetDto facet = new JobQueryFacetDto()
                {
                    Name = hr.SearchType

                };
                foreach (var hrValue in hr.Values)
                {
                    JobQueryFacetItemDto facetItem = new JobQueryFacetItemDto()
                    {          
                        Url = JobUrlHelper.MapFacetToUrl(jobQuery,facet.Name, hrValue.Key,IndustryUrl,LocationtUrl, TopLevelDomain),
                        Count = hrValue.Value.Value,
                        Label = hrValue.Key

                    };
                    facet.Facets.Add(facetItem);
                }
                rVal.Add(facet);

            }

            // map custom facets  
            // Note: currently not using nor supporting cloud talent custom long facet values
            foreach (CloudTalentSolution.CustomAttributeHistogramResult hr in searchJobsResponse.HistogramResults.CustomAttributeHistogramResults)
            {
                JobQueryFacetDto facet = new JobQueryFacetDto()
                {
                    Name = hr.Key

                };
                int index = 0;
                if (hr.StringValueHistogramResult != null)
                {
                    foreach (KeyValuePair<string, int?> facetInfo in hr.StringValueHistogramResult)
                    {

                        JobQueryFacetItemDto facetItem = new JobQueryFacetItemDto()
                        {
                            Url = JobUrlHelper.MapFacetToUrl(jobQuery,facet.Name, facetInfo.Key,IndustryUrl,LocationtUrl,TopLevelDomain),
                            Count = facetInfo.Value.Value,
                            Label = facetInfo.Key

                        };
                        facet.Facets.Add(facetItem);
                    }
                }
                rVal.Add(facet);
            }

            return rVal;
        }



        public static void MapCustomJobPostingAttributes(CloudTalentSolution.MatchingJob matchingJob, JobViewDto jobViewDto)
        {
            DateTime dateVal = DateTime.MinValue;
            // Short circuit if the job has no custom attributes 
            if (matchingJob.Job.CustomAttributes == null)
                return;
            // map application deadline 
            jobViewDto.ApplicationDeadlineUTC = Utils.FromUnixTimeInSeconds(MapCustomLongAttribute(matchingJob.Job.CustomAttributes, "ApplicationDeadlineUTC"));

            // map modify date 
            jobViewDto.ModifyDate = Utils.FromUnixTimeInSeconds(MapCustomLongAttribute(matchingJob.Job.CustomAttributes, "ModifyDate"));

            // map posting expiration date             
            jobViewDto.PostingExpirationDateUTC = Utils.FromUnixTimeInSeconds(MapCustomLongAttribute(matchingJob.Job.CustomAttributes, "PostingExpirationDateUTC"));

            // map experience level
            jobViewDto.ExperienceLevel = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "ExperienceLevel");

            // map education level 
            jobViewDto.EducationLevel = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "EducationLevel");

            // map employment type 
            jobViewDto.EmploymentType = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "EmploymentType");

            // map third party apply url
            jobViewDto.ThirdPartyApplyUrl = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "ThirdPartyApplyUrl");
 
            // map annual compensation
            jobViewDto.AnnualCompensation = MapCustomLongAttribute(matchingJob.Job.CustomAttributes, "AnnualCompensation");

            // map telecommute percentage 
            jobViewDto.TelecommutePercentage = MapCustomLongAttribute(matchingJob.Job.CustomAttributes, "TelecommutePercentage");

            // map third party apply flag  
            if (MapCustomLongAttribute(matchingJob.Job.CustomAttributes, "TelecommutePercentage") == 1)
                jobViewDto.ThirdPartyApply = true;
            else
                jobViewDto.ThirdPartyApply = false;
          
            // map industry
            jobViewDto.Industry = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "Industry");

            // map job category
            jobViewDto.JobCategory = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "JobCategory");

            // map skills
            if (matchingJob.Job.CustomAttributes.Keys.Contains("Skills") && matchingJob.Job.CustomAttributes["Skills"] != null)
            {
                foreach (string skill in matchingJob.Job.CustomAttributes["Skills"].StringValues)
                    jobViewDto.Skills.Add(skill);
            }
            // map country
            jobViewDto.Country = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "Country");
            // map province 
            jobViewDto.Province = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "Province");
            // map city
            jobViewDto.City = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "City");
            // map postal code 
            jobViewDto.PostalCode = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "PostalCode");
            // map street address 
            jobViewDto.StreetAddress = MapCustomStringAttribute(matchingJob.Job.CustomAttributes, "StreetAddress");
        }



        #endregion

        #region CC job -> cloud talent job mapping helpers

        /// <param name="jobPosting"></param>
        /// <returns></returns>
        static public CloudTalentSolution.Job CreateGoogleJob(UpDiddyDbContext db, JobPosting jobPosting)
        {
            // Set default application instructions as required by Google
            CloudTalentSolution.ApplicationInfo applicationInfo = new CloudTalentSolution.ApplicationInfo()
            {
                Instruction = "Apply Now!"
            };

            // Create custom job posting attributes 
            IDictionary<string, CloudTalentSolution.CustomAttribute> customAttributes = CreateGoogleJobCustomAttributes(db,jobPosting);

            // Set the jobs expire timestamp
            string ExpireTimestamp = Utils.GetTimestampAsString(jobPosting.PostingExpirationDateUTC);
            var jobToBeCreated = new CloudTalentSolution.Job()
            {
                RequisitionId = jobPosting.JobPostingGuid.ToString(),
                Title = jobPosting.Title,
                Description = jobPosting.Description,
                ApplicationInfo = applicationInfo,
                CompanyName = jobPosting.Company.CloudTalentUri,
                PostingExpireTime = ExpireTimestamp,
                Addresses = new List<string>()
                    {

                       JobPostingFactory.GetJobPostingLocation(jobPosting)

                    }
            };

            // Add google name if it exists (needed for updates)
            if (string.IsNullOrEmpty(jobPosting.CloudTalentUri) == false)
                jobToBeCreated.Name = jobPosting.CloudTalentUri;


            // Add custom attributes if they've been defined 
            if (customAttributes.Count > 0)
                jobToBeCreated.CustomAttributes = customAttributes;

            return jobToBeCreated;
        }

        #endregion

        #region Job indexing 

        static public bool AddJobToCloudTalent(UpDiddyDbContext db, CloudTalent ct, Guid jobPostingGuid)
        {
            try
            {
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuid(db, jobPostingGuid);
                // validate we have good data 
                if (jobPosting == null || jobPosting.Company == null)
                    return false;
                // validate the company is known to google, if not add it to the cloud talent 
                if (string.IsNullOrEmpty(jobPosting.Company.CloudTalentUri))
                    ct.IndexCompany(jobPosting.Company);

                // index the job to google 
                CloudTalentSolution.Job job = ct.IndexJob(jobPosting);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static public bool UpdateJobToCloudTalent(UpDiddyDbContext db, CloudTalent ct, Guid jobPostingGuid)
        {
            try
            {
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuid(db, jobPostingGuid);
                // validate we have good data 
                if (jobPosting == null || jobPosting.Company == null)
                    return false;
                // validate the company is known to google, if not add it to the cloud talent 
                if (string.IsNullOrEmpty(jobPosting.Company.CloudTalentUri))
                    ct.IndexCompany(jobPosting.Company);

                // index the job to google 
                CloudTalentSolution.Job job = ct.ReIndexJob(jobPosting);
                return true;
            }
            catch
            {
                return false;
            }
        }



        static public bool DeleteJobFromCloudTalent(UpDiddyDbContext db, CloudTalent ct, Guid jobPostingGuid)
        {
            try
            {
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuid(db, jobPostingGuid);
                // validate we have good data 
                if (jobPosting == null)
                    return false;
          
                // index the job to google 
                return ct.RemoveJobFromIndex(jobPosting);     
            }
            catch
            {
                return false;
            }
        }


        #endregion

        #region Helper functions 




        static private long MapCustomLongAttribute(IDictionary<string, CloudTalentSolution.CustomAttribute> attributes, string attributeName)
        {
            long rVal = 0;

            if (attributes.Keys.Contains(attributeName) &&
                 attributes[attributeName] != null &&
                 attributes[attributeName].LongValues != null
               )
                rVal = attributes[attributeName].LongValues[0].Value;

            return rVal;
        }


        static private string MapCustomStringAttribute( IDictionary<string, CloudTalentSolution.CustomAttribute> attributes,  string attributeName )
        {
            string rVal = string.Empty;

            if ( attributes.Keys.Contains(attributeName) &&
                 attributes[attributeName] != null &&
                 attributes[attributeName].StringValues != null
               )
                rVal = attributes[attributeName].StringValues[0].ToString();

                return rVal;
        }



        /// <summary>
        ///  Create list of custom attributes for cloud talent job posting 
        /// </summary>
        /// <param name="jobPosting"></param>
        /// <returns></returns>
        static private IDictionary<string, CloudTalentSolution.CustomAttribute> CreateGoogleJobCustomAttributes(UpDiddyDbContext db, JobPosting jobPosting)
        {

            IDictionary<string, CloudTalentSolution.CustomAttribute> rVal = new Dictionary<string, CloudTalentSolution.CustomAttribute>();

            // Add application deadline date as a long so it can be queried with boolean <= logic  
            CloudTalentSolution.CustomAttribute ApplicationDeadlineUTC = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                LongValues = new List<long?>() { Utils.ToUnixTimeInSeconds(jobPosting.ApplicationDeadlineUTC) }

            };
            rVal.Add("ApplicationDeadlineUTC", ApplicationDeadlineUTC);

            // Add experience level if its been specified 
            if (jobPosting.ExperienceLevel != null)
            {
                CloudTalentSolution.CustomAttribute ExperienceLevel = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.ExperienceLevel.DisplayName }

                };
                rVal.Add("ExperienceLevel", ExperienceLevel);
            }

            // Add education level if its been specified 
            if (jobPosting.EducationLevel != null)
            {
                CloudTalentSolution.CustomAttribute EducationLevel = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.EducationLevel.Level }

                };
                rVal.Add("EducationLevel", EducationLevel);
            }

            // Add compensation normalized to annual values 
            if (jobPosting.Compensation > 0 && jobPosting.CompensationType != null)
            {

                long? AnnualSalary = CompensationTypeFactory.AnnualCompensation(jobPosting.Compensation, jobPosting.CompensationType);
                CloudTalentSolution.CustomAttribute AnnualCompensation = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    LongValues = new List<long?>() { AnnualSalary }
                };
                rVal.Add("AnnualCompensation", AnnualCompensation);
            }

            // Add employment type
            if (jobPosting.EmploymentType != null)
            {
 
                CloudTalentSolution.CustomAttribute EmploymentType = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.EmploymentType.Name }
                };
                rVal.Add("EmploymentType", EmploymentType);
            }

            // Add industry type
            if (jobPosting.Industry != null)
            {

                CloudTalentSolution.CustomAttribute Industry = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.Industry.Name }
                };
                rVal.Add("Industry", Industry);
            }

            // Add job category 
            if (jobPosting.JobCategory != null)
            {

                CloudTalentSolution.CustomAttribute JobCategory = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = new List<string>() { jobPosting.JobCategory.Name }
                };
                rVal.Add("JobCategory", JobCategory);
            }

            // Add posting expiration date as a long so it can be queried with boolean <= logic  
            CloudTalentSolution.CustomAttribute PostingExpirationDateUTC = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                LongValues = new List<long?>() { Utils.ToUnixTimeInSeconds(jobPosting.PostingExpirationDateUTC) }

            };
            rVal.Add("PostingExpirationDateUTC", PostingExpirationDateUTC);

            // Add posting modify date as a long so it can be queried with boolean <= logic  
            CloudTalentSolution.CustomAttribute ModifyDate = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                LongValues = new List<long?>() { Utils.ToUnixTimeInSeconds(jobPosting.ModifyDate.Value) }
            };
            rVal.Add("ModifyDate", ModifyDate);

            // Add posting thirdparty apply as a long so it can be queried with boolean <= logic  
            long ApplyFlag = jobPosting.ThirdPartyApply == true ? 1 : 0;
            CloudTalentSolution.CustomAttribute ThirdPartyApply = new CloudTalentSolution.CustomAttribute()
            {            
                Filterable = true,
                LongValues = new List<long?>() { ApplyFlag }
            };
            rVal.Add("ThirdPartyApply", ThirdPartyApply);

            // Add posting thirdparty url so it can be returned as part of the job        
            CloudTalentSolution.CustomAttribute ThirdPartyApplyUrl = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { jobPosting.ThirdPartyApplicationUrl }
            };
            rVal.Add("ThirdPartyApplyUrl", ThirdPartyApplyUrl);

            // Add telecommute percent as a long so it can be queried with boolean <= logic   
            CloudTalentSolution.CustomAttribute TelecommutePercentage = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                LongValues = new List<long?>() { jobPosting.TelecommutePercentage }
            };
            rVal.Add("TelecommutePercentage", TelecommutePercentage);

            var jobSkills = JobPostingFactory.GetPostingSkills(db, jobPosting);
            List<string> skillsList = new List<string>();
            foreach ( JobPostingSkill jps in jobSkills)
            {
                if (jps.Skill != null)
                    skillsList.Add(jps.Skill.SkillName.Trim());
            }
   
            if ( skillsList.Count > 0 )
            {
                CloudTalentSolution.CustomAttribute Skills = new CloudTalentSolution.CustomAttribute()
                {
                    Filterable = true,
                    StringValues = skillsList
                };
                rVal.Add("Skills", Skills);
            }
            // Index Country 
            CloudTalentSolution.CustomAttribute Country = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { jobPosting.Country }
            };
            rVal.Add("Country", Country);
            // Index Province 
            CloudTalentSolution.CustomAttribute Province = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { jobPosting.Province }
            };
            rVal.Add("Province", Province);
            // Index Postal code  
            CloudTalentSolution.CustomAttribute PostalCode = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { jobPosting.PostalCode }
            };
            rVal.Add("PostalCode", PostalCode);
            // Index City
            CloudTalentSolution.CustomAttribute City = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { jobPosting.City }
            };
            rVal.Add("City", City);
            // Index street address 
            CloudTalentSolution.CustomAttribute StreetAddress = new CloudTalentSolution.CustomAttribute()
            {
                Filterable = true,
                StringValues = new List<string>() { jobPosting.StreetAddress }
            };
            rVal.Add("StreetAddress", StreetAddress);
            return rVal; 
        }

    }


        #endregion


    
}
