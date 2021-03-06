﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using UpDiddy.ViewModels;
using Microsoft.AspNetCore.Authorization;
using UpDiddy.Authentication;
using X.PagedList;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Claims;
using UpDiddyLib.Helpers;

namespace UpDiddy.Controllers
{
    public class JobsController : BaseController
    {
        private readonly IHostingEnvironment _env;
        private readonly int _activeJobCount = 0;

        public JobsController(IApi api,
        IConfiguration configuration,
        IHostingEnvironment env)
         : base(api,configuration)
        {
            _env = env;
            int.TryParse(api.GetActiveJobCountAsync().Result.Description, out _activeJobCount);
        }

        /*
        [HttpGet("[controller]")]
        public async Task<IActionResult> Index()
        {
            ViewBag.ActiveJobCount = _activeJobCount;

            //get pageCount from Configuration file
            int pageCount = _configuration.GetValue<int>("Pagination:PageCount");

            JobSearchResultDto jobSearchResultDto = null;
            Dictionary<Guid, Guid> favoritesMap = new Dictionary<Guid, Guid>();

            string Keywords, Location, Province, City;
            Keywords = Location = Province = City = string.Empty;

            string queryParametersString = string.Empty;
            var queryParameterList = Request.Query.ToArray();

            foreach (var queryParameter in queryParameterList)
            {
                if ((queryParameter.Key == "datepublished" || queryParameter.Key == "employmenttype"
                       || queryParameter.Key == "city" || queryParameter.Key == "jobcategory"
                       || queryParameter.Key == "industry" || queryParameter.Key == "keywords"
                       || queryParameter.Key == "location" || queryParameter.Key == "province"
                       || queryParameter.Key == "companyname") && !string.IsNullOrEmpty(queryParameter.Value))
                {
                    //remove anchor brackets
                    Regex removeBrackets = new Regex(@"[{}<>]");
                    var filteredQueryParameterValue = removeBrackets.Replace(queryParameter.Value, string.Empty);

                    //trim string to max of 100 characters
                    filteredQueryParameterValue = filteredQueryParameterValue.Length > 100 ? filteredQueryParameterValue.Substring(0, 99)
                                                                                    : filteredQueryParameterValue;

                    if (string.IsNullOrEmpty(queryParametersString) || string.IsNullOrWhiteSpace(queryParametersString))
                    {
                        queryParametersString += $"?{queryParameter.Key}={filteredQueryParameterValue}";
                    }
                    else
                    {
                        queryParametersString += $"&{queryParameter.Key}={filteredQueryParameterValue}";
                    }

                    if (queryParameter.Key == "keywords")
                        Keywords = filteredQueryParameterValue;
                    else if (queryParameter.Key == "location")
                        Location = filteredQueryParameterValue;
                    else if (queryParameter.Key == "city")
                        City = filteredQueryParameterValue;
                    else if (queryParameter.Key == "province")
                        Province = filteredQueryParameterValue;
                }
            }

            if (Keywords != null)
                Keywords = Keywords.Trim();

            if (Location != null)
                Location = Location.Trim();

            if (Province != null)
                Province = Province.Trim();

            if (City != null)
                City = City.Trim();

            // If no location parameter is supplied, but city and/or state is, prefill location
            // input box with city and/or state.
            if (string.IsNullOrEmpty(Location) && (!string.IsNullOrEmpty(Province) || !string.IsNullOrEmpty(City)))
            {
                Location = (string.IsNullOrEmpty(City) ? string.Empty : City) +
                    ((!string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(Province)) ? ", " : string.Empty) +
                    (string.IsNullOrEmpty(Province) ? string.Empty : Province);
            }

            int.TryParse(Request.Query["page"], out int page);

            try
            {
                jobSearchResultDto = await _Api.GetJobsByLocation(
                                      queryParametersString);

                // quick fix to increase search radius when search results are limited (only when a keyword and/or location search is executed).
                // don't need to be concerned with checking if this param already exists because it would have been filtered out by the above logic.
                // a better solution would be to consolidate the query string logic into a helper method and reuse it here.
                if (jobSearchResultDto.JobCount < 10 & (!string.IsNullOrWhiteSpace(queryParametersString) || queryParametersString.Contains("?")))
                {
                    queryParametersString += "&search-radius=50";
                    jobSearchResultDto = await _Api.GetJobsByLocation(queryParametersString);
                }

                if (User.Identity.IsAuthenticated)
                    favoritesMap = await _Api.JobFavoritesByJobGuidAsync(jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount).Select(job => job.JobPostingGuid).ToList());
            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }

            if (jobSearchResultDto == null)
                return NotFound();

            // when applying a new a histogram the page should be reset to the first page
            Regex matchPagingQueryStringParameter = new Regex(@"page=.+?(?=\w)");
            queryParametersString = matchPagingQueryStringParameter.Replace(queryParametersString, string.Empty);

            ViewBag.QueryUrl = queryParametersString;

            JobSearchViewModel jobSearchViewModel = new JobSearchViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount),
                FavoritesMap = favoritesMap,
                Facets = jobSearchResultDto.Facets,
                Keywords = Keywords,
                Location = Location,
                JobQueryForAlert = jobSearchResultDto.JobQueryForAlert
            };

            return View("Index", jobSearchViewModel);
        }

        [HttpGet]
        [Route("jobs/{JobGuid}")]
        [Route("jobs/{industry}/{category}/{country}/{state}/{city}/{JobGuid}")]
        public IActionResult RedirectOldJobsUrl(Guid JobGuid)
        {

            var path = Request.Path.Value;
            return RedirectToActionPermanent("JobAsync", "Jobs", new { JobGuid = JobGuid });
        }


        [HttpGet]
        [Route("job/{JobGuid}")]
        public  async Task<IActionResult> JobRedirect(Guid JobGuid)
        {

            string url = string.Empty;
            JobPostingDto job = null;
            try
            {
                job = await _Api.GetJobAsync(JobGuid, GoogleCloudEventsTrackingDto.Build(HttpContext.Request.Query, UpDiddyLib.Shared.GoogleJobs.ClientEventType.View));
                if (job != null)
                {
                    url = job.SemanticJobPath;
                    string queryParams = Request.QueryString.ToString();
                    if (!string.IsNullOrEmpty(queryParams))
                        url += queryParams;
                    return RedirectPermanent(url);
                }
                else
                {
                    return RedirectPermanent("/jobs?JobExpired=" + JobGuid.ToString());
                }

            }
            catch (ApiException e)
            {
                return RedirectPermanent("/jobs?JobExpired=" + JobGuid.ToString()); 
            }

            //cookie the user with the source of the job detail Page view  
            if (Request != null && Request.Query != null && string.IsNullOrEmpty(Request.Query["Source"].ToString()) == false)
            {
                SetCookie(JobGuid.ToString(), Request.Query["Source"].ToString(), 262800);
            }


            Response.StatusCode = 301;
            Response.Redirect(url,true);

            return RedirectPermanent("/jobs?JobExpired=" + JobGuid.ToString());
        }



        [HttpGet] 
        [Route("job/{industry}/{category}/{country}/{state}/{city}/{JobGuid}")]
        public async Task<IActionResult> JobAsync(Guid JobGuid)
        {
            var path = Request.Path.Value;
            JobPostingDto job = null;
            try
            {
                job = await _Api.GetJobAsync(JobGuid, GoogleCloudEventsTrackingDto.Build(HttpContext.Request.Query, UpDiddyLib.Shared.GoogleJobs.ClientEventType.View));

                if (job.JobStatus == (int)JobPostingStatus.Draft)
                {
                    BasicResponseDto ResponseDto = new BasicResponseDto() { StatusCode = 404, Description = "Jobposting not found!" };
                    throw new ApiException(new System.Net.Http.HttpResponseMessage(), ResponseDto);
                }
            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (404):
                        // Try to find as an expired job for representatives search.
                        //todo: See if we can allow expired/deleted jobs to get here to decide to show representatives and save an API call.
                        try
                        {
                            job = await _Api.GetExpiredJobAsync(JobGuid);
                        }
                        catch (ApiException ae)
                        {
                            return StatusCode(ae.ResponseDto.StatusCode);
                        }

                        int pageCount = _configuration.GetValue<int>("Pagination:PageCount");
                        string location = string.Empty;
                        JobSearchResultDto jobSearchResultDto;

                        if (job is null)
                        {
                            // Show all jobs.
                            jobSearchResultDto = await _Api.GetJobsByLocation(null);
                        }
                        else
                        {

                            // Show representatives.
                            location = job?.City + ", " + job?.Province;
                            var queryParametersString = $"?{job.Title}&{location}";

                            jobSearchResultDto = await _Api.GetJobsByLocation(queryParametersString);

                            jobSearchResultDto = await _Api.GetJobsByLocation(queryParametersString);
                        }

                        if (jobSearchResultDto == null)
                            return NotFound();

                        jobSearchResultDto.JobQueryForAlert.Keywords = job?.Title ?? string.Empty;
                        jobSearchResultDto.JobQueryForAlert.Location = location;
                        var jobSearchViewModel = new JobSearchViewModel()
                        {
                            Keywords = job?.Title ?? string.Empty,
                            Location = location,
                            JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount),
                            JobQueryForAlert = jobSearchResultDto.JobQueryForAlert
                        };

                        // Remove the expired job link from the search provider's index.
                        Response.StatusCode = 404;

                        ViewBag.QueryUrl = string.Empty;
                        return View("Index", jobSearchViewModel);
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }

            if (job == null)
                return NotFound();

            // check to see if the inbound url matches the semantic url
            if (job.SemanticJobPath.ToLower() != Request.Path.Value.ToLower())
            {
                // if it does not match, redirect to the semantic url
                return RedirectPermanent(job.SemanticJobPath.ToLower());
            }

            Guid? jobFavoriteGuid = null;
            if (User.Identity.IsAuthenticated)
            {
                var favorite = await _Api.JobFavoritesByJobGuidAsync(new List<Guid>() { job.JobPostingGuid.Value });
                if (favorite.Any())
                    jobFavoriteGuid = favorite.First().Value;
            }

            List<Guid> SimilarJobsFavoritesGuids = new List<Guid>();
            foreach (JobViewDto JobViewDto in job.SimilarJobs.Jobs)
            {
                SimilarJobsFavoritesGuids.Add(JobViewDto.JobPostingGuid);
            }

            Dictionary<Guid, Guid> SimilarJobsFavorites = new Dictionary<Guid, Guid>();
            if (User.Identity.IsAuthenticated)
            {
                SimilarJobsFavorites = await _Api.JobFavoritesByJobGuidAsync(SimilarJobsFavoritesGuids);
            }

            JobViewDto JobToBeRemoved = null;

            foreach (JobViewDto result in job.SimilarJobs.Jobs)
            {
                if (result.JobPostingGuid == job.JobPostingGuid)
                    JobToBeRemoved = result;
            }

            if (JobToBeRemoved != null)
            {
                job.SimilarJobs.Jobs.Remove(JobToBeRemoved);
            }

            JobDetailsViewModel jdvm = new JobDetailsViewModel
            {
                RequestId = job.RequestId,
                ClientEventId = job.ClientEventId,
                JobPostingFavoriteGuid = jobFavoriteGuid,
                Name = job.Title,
                JobPostingGuid = job.JobPostingGuid,
                Company = job.Company?.CompanyName,
                PostedDate = job.PostingDateUTC == null ? string.Empty : job.PostingDateUTC.ToLocalTime().ToString(),
                Location = $"{job.City}, {job.Province}, {job.Country}",
                PostingId = job.JobPostingGuid?.ToString(),
                EmployeeType = job.EmploymentType?.Name,
                Summary = job.Description,
                ThirdPartyIdentifier = job.ThirdPartyIdentifier,
                CompanyBoilerplate = job.Company.JobPageBoilerplate,
                IsThirdPartyJob = job.ThirdPartyApply,
                MetaDescription = job.MetaDescription,
                MetaTitle = job.MetaTitle,
                MetaKeywords = job.MetaKeywords,
                SimilarJobsSearchResult = job.SimilarJobs,
                City = job.City,
                Province = job.Province,
                SimilarJobsFavorites = SimilarJobsFavorites,
                Skills = job.JobPostingSkills != null ? job.JobPostingSkills.Select(x => x.SkillName).ToList() : null,
                LogoUrl = job?.Company?.LogoUrl != null ? _configuration["CareerCircle:AssetBaseUrl"] + "Company/" + job.Company.LogoUrl : string.Empty,
      
            };

            jdvm.ApplyUrl = "/jobs/apply/" + jdvm.PostingId;
            string queryParams = Request.QueryString.ToString();
            if (!string.IsNullOrEmpty(queryParams))
                jdvm.ApplyUrl += queryParams;


            // Display subscriber info if it exists
            if (job.Recruiter.Subscriber != null)
            {
                jdvm.ContactEmail = job.Recruiter.Subscriber?.Email;
                jdvm.ContactName = string.Join(' ',
                    new[] {
                        job.Recruiter.Subscriber?.FirstName,
                        job.Recruiter.Subscriber?.LastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                );
                jdvm.ContactPhone = job.Recruiter.Subscriber?.PhoneNumber;
            }
            else // Use recruiter info in no subscriber exists
            {
                jdvm.ContactEmail = job.Recruiter?.Email;
                jdvm.ContactName = string.Join(' ',
                    new[] {
                        job.Recruiter?.FirstName,
                        job.Recruiter?.LastName }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                );
                jdvm.ContactPhone = job.Recruiter?.PhoneNumber;
            }

            //check if user is logged in
            if (this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value != null)
            {
                var subscriber = await _Api.SubscriberAsync(Guid.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value), false);
                jdvm.LoggedInSubscriberGuid = subscriber.SubscriberGuid;
                jdvm.LoggedInSubscriberEmail = subscriber.Email;
                jdvm.LoggedInSubscriberName = subscriber.FirstName + " " + subscriber.LastName;
                await _Api.RecordSubscriberJobViewAction(JobGuid, GetSubscriberGuid());
            }

            //update job as viewed if there is referrer code
            if (Request.Cookies["referrerCode"] != null)
                await _Api.UpdateJobViewed( Utils.AlphaNumeric(Request.Cookies["referrerCode"].ToString(),_maxCookieLength));

            return View("JobDetails", jdvm);
        }




        [Authorize]
        [LoadSubscriber(isHardRefresh: true, isSubscriberRequired: true)]
        [HttpGet("[controller]/apply/{JobGuid}")]
        public async Task<IActionResult> ApplyAsync(Guid JobGuid)
        {
            JobPostingDto job = null;
            try
            {
                job = await _Api.GetJobAsync(JobGuid);
            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }

            if (job == null)
                return NotFound();


            var trackingDto = await _Api.RecordClientEventAsync(JobGuid, GoogleCloudEventsTrackingDto.Build(HttpContext.Request.Query, UpDiddyLib.Shared.GoogleJobs.ClientEventType.Application_Start));
            await _Api.RecordSubscriberApplyAction(JobGuid, this.subscriber.SubscriberGuid.Value);
            return View("Apply", new JobApplicationViewModel()
            {
                RequestId = trackingDto?.RequestId,
                ClientEventId = trackingDto?.ClientEventId,
                Email = this.subscriber.Email,
                FirstName = string.IsNullOrEmpty(this.subscriber.FirstName) ? string.Empty : this.subscriber.FirstName,
                LastName = string.IsNullOrEmpty(this.subscriber.LastName) ? string.Empty : this.subscriber.LastName,
                Job = job,
                JobPostingGuid = JobGuid,
                HasResumeOnFile = this.subscriber.Files.Count > 0
            });
        }

        [Authorize]
        [LoadSubscriber(isHardRefresh: false, isSubscriberRequired: true)]
        [HttpPost("[controller]")]
        public async Task<IActionResult> SubmitApplicationAsync(JobApplicationViewModel JobApplicationViewModel)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(500);
            }

            if (this.subscriber.Files.Count == 0 && JobApplicationViewModel.UploadedResume == null)
                return BadRequest();

            JobPostingDto job = null;
            try
            {
                job = await _Api.GetJobAsync(JobApplicationViewModel.JobPostingGuid);
            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }

            if (job == null)
                return NotFound();

            this.subscriber.FirstName = JobApplicationViewModel.FirstName;
            this.subscriber.LastName = JobApplicationViewModel.LastName;

            JobApplicationDto jadto = new JobApplicationDto()
            {
                JobPosting = job,
                Subscriber = this.subscriber,
                CoverLetter = JobApplicationViewModel.CoverLetter
            };


            var cookieKey = job.JobPostingGuid.ToString();
            // Check to see if their is a parter source cookie for this job
            if (Request != null && Request.Cookies != null && Request.Cookies[cookieKey] != null && string.IsNullOrEmpty(Request.Cookies[cookieKey].ToString()) == false)
            {
                string jobPartnerSource = Utils.AlphaNumeric(Request.Cookies[job.JobPostingGuid.ToString()].ToString(), _maxCookieLength);
                PartnerDto partner = await _Api.GetPartnerByNameAsync(jobPartnerSource);
                if (partner == null )
                {
                    partner = new PartnerDto()
                    {
                        Name = jobPartnerSource
                    };
                }
                jadto.Partner = partner;
                // Consume the cookie, hehe that's funny
                RemoveCookie(job.JobPostingGuid.ToString());
            }



            BasicResponseDto Response = null;

            CompletedJobApplicationViewModel cjavm = new CompletedJobApplicationViewModel();
            try
            {
                Response = await _Api.ApplyToJobAsync(jadto);

                await _Api.RecordClientEventAsync(JobApplicationViewModel.JobPostingGuid, new GoogleCloudEventsTrackingDto()
                {
                    RequestId = JobApplicationViewModel.RequestId,
                    ParentClientEventId = JobApplicationViewModel.ClientEventId,
                    Type = UpDiddyLib.Shared.GoogleJobs.ClientEventType.Application_Finish
                });

                cjavm.JobApplicationStatus = CompletedJobApplicationViewModel.ApplicationStatus.Success;
            }
            catch (ApiException e)
            {
                cjavm.JobApplicationStatus = CompletedJobApplicationViewModel.ApplicationStatus.Failed;
                cjavm.Description = e.ResponseDto.Description;
            }


            return View("Finish", cjavm);
        }

        [HttpGet("Browse-Jobs")]
        public async Task<IActionResult> BrowseAsync()
        {

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _Api.GetJobsUsingRoute();
            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }


            BrowseJobsViewModel bjvm = new BrowseJobsViewModel();
            bjvm.ViewModels = new List<BrowseJobsByTypeViewModel>
            {
                GetStateViewModel(jobSearchResultDto.Facets, "/browse-jobs-location/us", "Select Desired State", true)
                //GetIndustryViewModel(jobSearchResultDto.Facets, "/browse-jobs-industry", "Select Desired Industry", true),
                //GetCategoryViewModel(jobSearchResultDto.Facets, "/browse-jobs-category", false, "Select Desired Category", true)
            };

            return View("Browse", bjvm);
        }


        [HttpGet("browse-jobs-location/{country?}")]
        [HttpGet("browse-jobs-location/{country}/{page:int?}")]
        [HttpGet("browse-jobs-location/{country}/{state?}/{page:int?}")]
        [HttpGet("browse-jobs-location/{country}/{state}/{city?}/{page:int?}")]
        [HttpGet("browse-jobs-location/{country}/{state}/{city}/{industry?}/{page:int?}")]
        [HttpGet("browse-jobs-location/{country}/{state}/{city}/{industry}/{category?}/{page:int?}")]
        public async Task<IActionResult> BrowseJobsByLocationAsync(
            string country,
            string state,
            string city,
            string industry,
            string category,
            int page)
        {
            // If user types in root url, default to US.
            if (string.IsNullOrEmpty(country))
                return RedirectPermanent($"{Request.Path}/us");


            if (!string.IsNullOrEmpty(country) &&
                !string.IsNullOrEmpty(state) &&
                !string.IsNullOrEmpty(city) &&
                !string.IsNullOrEmpty(industry) &&
                !string.IsNullOrEmpty(category) &&
                page == 0)
                return RedirectPermanent($"{Request.Path}/1");


            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _Api.GetJobsUsingRoute(
                    country,
                    state,
                    city,
                    industry?.Replace("-", "+"),
                    category?.Replace("-", "+"),
                    page);
            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }

            if (page > jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0) || page < 0)
                return NotFound();

            if (jobSearchResultDto == null || !IsValidUrl(jobSearchResultDto, country, state, city, industry, category))
                return NotFound();

            int pageCount = _configuration.GetValue<int>("Pagination:PageCount");

            BrowseJobsByTypeViewModel jobSearchViewModel = new BrowseJobsByTypeViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount),
                CurrentPage = page,
                NumberOfPages = jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0)
            };

            if (User.Identity.IsAuthenticated)
            {
                jobSearchViewModel.FavoritesMap = await _Api.JobFavoritesByJobGuidAsync(jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount).Select(job => job.JobPostingGuid).ToList());
            }

            // Google seems to be capping the number of results at 500, so we account for that here.
            if (jobSearchViewModel.NumberOfPages > 500)
                jobSearchViewModel.NumberOfPages = 500;

            DeterminePaginationRange(ref jobSearchViewModel);

            Uri RequestUri = new Uri(($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}"));

            BreadcrumbViewModel BreadcrumbViewModel = CreateBreadcrumbs(RequestUri, jobSearchResultDto);
            jobSearchViewModel.Breadcrumbs = BreadcrumbViewModel;

            // User has reached the end of the browse flow, so present results.
            if (!string.IsNullOrEmpty(category) || page != 0)
            {
                if (string.IsNullOrEmpty(state))
                    jobSearchViewModel.Header = "United States";
                else if (string.IsNullOrEmpty(city))
                {
                    string StateName = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                    jobSearchViewModel.Header = StateName.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateName)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateName);
                }
                else if (string.IsNullOrEmpty(industry))
                    jobSearchViewModel.Header = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                else if (string.IsNullOrEmpty(category))
                    jobSearchViewModel.Header = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                else if (!string.IsNullOrEmpty(category))
                    jobSearchViewModel.Header = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                jobSearchViewModel.BaseUrl = AssembleBaseLocationUrl(country, state, city, industry, category);
                return View("BrowseByType", jobSearchViewModel);
            }

            // Return state view if user has only specified country

            if (string.IsNullOrEmpty(state))
            {
                BrowseJobsByTypeViewModel bjbtvm = GetStateViewModel(jobSearchResultDto.Facets, Request.Path, "Select Desired State", false, BreadcrumbViewModel);
                return View("BrowseByType", bjbtvm);
            }

            string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
            StateLabel = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel);

            if (string.IsNullOrEmpty(city))
            {

                BrowseJobsByTypeViewModel bjbtvm = GetCityViewModel(jobSearchResultDto.Facets, Request.Path, StateLabel, false, false, BreadcrumbViewModel);
                if (bjbtvm == null)
                    return RedirectPermanent(Request.Path + "/1");
                return View("BrowseByType", bjbtvm);
            }

            string CityLabel = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

            if (string.IsNullOrEmpty(industry))
            {
                BrowseJobsByTypeViewModel bjbtvm = GetIndustryViewModel(jobSearchResultDto.Facets, Request.Path, CityLabel, false, BreadcrumbViewModel);
                if (bjbtvm == null)
                    return RedirectPermanent(Request.Path + "/1");
                return View("BrowseByType", bjbtvm);
            }
            string IndustryLabel = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

            if (string.IsNullOrEmpty(category))
            {

                BrowseJobsByTypeViewModel bjbtvm = GetCategoryViewModel(jobSearchResultDto.Facets, Request.Path, IndustryLabel, true, false, BreadcrumbViewModel);
                if (bjbtvm == null)
                    return RedirectPermanent(Request.Path + "/1");
                return View("BrowseByType", bjbtvm);

            }

            return NotFound();



        }

        private BreadcrumbViewModel CreateBreadcrumbs(
            Uri RequestUri,
            JobSearchResultDto jobSearchResultDto)
        {
            List<string> UriOrder = new List<string>();

            switch (RequestUri.Segments[1].Split("/")[0])
            {
                case "browse-jobs-location":
                    UriOrder.Add("browse-jobs-location");
                    UriOrder.Add("country");
                    UriOrder.Add("state");
                    UriOrder.Add("city");
                    UriOrder.Add("industry");
                    UriOrder.Add("jobcategory");
                    UriOrder.Add("page");
                    break;
                case "browse-jobs-industry":
                    UriOrder.Add("browse-jobs-industry");
                    UriOrder.Add("industry");
                    UriOrder.Add("jobcategory");
                    UriOrder.Add("country");
                    UriOrder.Add("state");
                    UriOrder.Add("city");
                    UriOrder.Add("page");
                    break;
                case "browse-jobs-category":
                    UriOrder.Add("browse-jobs-category");
                    UriOrder.Add("jobcategory");
                    UriOrder.Add("industry");
                    UriOrder.Add("country");
                    UriOrder.Add("state");
                    UriOrder.Add("city");
                    UriOrder.Add("page");
                    break;
            }


            BreadcrumbViewModel BreadcrumbViewModel = new BreadcrumbViewModel
            {
                Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem
                    {
                        PageName = "Browse Jobs",
                        Url = "/browse-jobs"
                    }
                }
            };
            for (int i = 1; i < RequestUri.Segments.Length; i++)
            {
                string Segment = RequestUri.Segments[i].Split("/")[0];

                switch (UriOrder[i - 1])
                {
                    case "browse-jobs-location":
                        BreadcrumbViewModel.Breadcrumbs.Add(new BreadcrumbItem { PageName = "Browse Jobs By Location", Url = "/" + Segment });
                        break;
                    case "browse-jobs-industry":
                        BreadcrumbViewModel.Breadcrumbs.Add(new BreadcrumbItem { PageName = "Browse Jobs By Industry", Url = "/" + Segment });
                        break;
                    case "browse-jobs-category":
                        BreadcrumbViewModel.Breadcrumbs.Add(new BreadcrumbItem { PageName = "Browse Jobs By Category", Url = "/" + Segment });
                        break;
                    case "country":
                        if (Regex.IsMatch(Segment, @"^\d+$")) break;
                        BreadcrumbViewModel.Breadcrumbs.Add(new BreadcrumbItem { PageName = "United States", Url = BreadcrumbViewModel.Breadcrumbs.Last().Url + "/us" });
                        break;
                    case "state":
                        if (Regex.IsMatch(Segment, @"^\d+$")) break;
                        string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                        StateLabel = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel);
                        BreadcrumbViewModel.Breadcrumbs.Add(new BreadcrumbItem { PageName = StateLabel, Url = BreadcrumbViewModel.Breadcrumbs.Last().Url + "/" + Segment });
                        break;
                    case "city":
                        if (Regex.IsMatch(Segment, @"^\d+$")) break;
                        string CityLabel = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                        BreadcrumbViewModel.Breadcrumbs.Add(new BreadcrumbItem { PageName = CityLabel, Url = BreadcrumbViewModel.Breadcrumbs.Last().Url + "/" + Segment });
                        break;
                    case "industry":
                        if (Regex.IsMatch(Segment, @"^\d+$")) break;
                        string IndustryLabel = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                        BreadcrumbViewModel.Breadcrumbs.Add(new BreadcrumbItem { PageName = IndustryLabel, Url = BreadcrumbViewModel.Breadcrumbs.Last().Url + "/" + Segment });
                        break;
                    case "jobcategory":
                        if (Regex.IsMatch(Segment, @"^\d+$")) break;
                        string CategorryLabel = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                        BreadcrumbViewModel.Breadcrumbs.Add(new BreadcrumbItem { PageName = CategorryLabel, Url = BreadcrumbViewModel.Breadcrumbs.Last().Url + "/" + Segment });
                        break;

                }

            }

            return BreadcrumbViewModel;

        }


        #region Retrieve Section ViewModels

        public BrowseJobsByTypeViewModel GetStateViewModel(
            List<JobQueryFacetDto> Facets,
            string Path, string Header,
            bool HideAllLink = false,
            BreadcrumbViewModel BreadcrumbViewModel = null)
        {
            JobQueryFacetDto jqfdto = FindNeededFacet("admin_1", Facets);
            List<DisplayItem> StateLocations = new List<DisplayItem>();
            jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
            foreach (JobQueryFacetItemDto JobQueryFacet in jqfdto.Facets)
            {
                UpDiddyLib.Helpers.Utils.State State;
                Enum.TryParse(JobQueryFacet.Label.ToUpper(), out State);
                string StateName = UpDiddyLib.Helpers.Utils.GetState(State);
                StateLocations.Add(new DisplayItem
                {
                    Label = $"{UpDiddyLib.Helpers.Utils.ToTitleCase(StateName)}",
                    Url = $"{Path}/{JobQueryFacet.Label.ToLower()}",
                    Count = $"{JobQueryFacet.Count}"
                });
            }

            BrowseJobsByTypeViewModel bjbtvm = new BrowseJobsByTypeViewModel()
            {
                Items = StateLocations,
                Header = "Select Desired State:",
                HideAllLink = HideAllLink,
                Breadcrumbs = BreadcrumbViewModel
            };
            return bjbtvm;
        }

        public BrowseJobsByTypeViewModel GetCityViewModel(
            List<JobQueryFacetDto> Facets,
            string Path,
            string Header,
            bool ShowResults = false,
            bool HideAllLink = false,
            BreadcrumbViewModel BreadcrumbViewModel = null)
        {
            JobQueryFacetDto jqfdto = FindNeededFacet("city", Facets);


            List<DisplayItem> LocationsCities = new List<DisplayItem>();
            jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
            foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
            {
                Regex rgx = new Regex("[^a-zA-Z]");
                LocationsCities.Add(new DisplayItem
                {
                    Label = $"{FacetItem.Label.Split(",")[0]}",
                    Url = $"{Request.Path}/{rgx.Replace(FacetItem.Label.Split(",")[0].ToLower(), "-")}" + (ShowResults ? "/1" : string.Empty),
                    Count = $"{FacetItem.Count}"
                });
            }


            BrowseJobsByTypeViewModel bjbtvm = new BrowseJobsByTypeViewModel()
            {
                Items = LocationsCities,
                Header = Header,
                Breadcrumbs = BreadcrumbViewModel
            };
            return bjbtvm;
        }

        public BrowseJobsByTypeViewModel GetIndustryViewModel(
            List<JobQueryFacetDto> Facets,
            string Path,
            string Header,
            bool HideAllLink = false,
            BreadcrumbViewModel BreadcrumbViewModel = null)
        {
            JobQueryFacetDto jqfdto = FindNeededFacet("industry", Facets);

            if (jqfdto == null)
                return null;

            List<DisplayItem> Industries = new List<DisplayItem>();

            foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
            {
                Industries.Add(new DisplayItem
                {
                    Label = $"{FacetItem.Label}",
                    Url = $"{Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                    Count = $"{FacetItem.Count}"
                });
            }

            return new BrowseJobsByTypeViewModel() { Items = Industries, Header = Header, HideAllLink = HideAllLink, Breadcrumbs = BreadcrumbViewModel };
        }

        public BrowseJobsByTypeViewModel GetCategoryViewModel(
            List<JobQueryFacetDto> Facets,
            string Path,
            string Header,
            bool ShowResults = false,
            bool HideAllLink = false,
            BreadcrumbViewModel BreadcrumbViewModel = null)
        {
            JobQueryFacetDto jqfdto = FindNeededFacet("jobcategory", Facets);

            if (jqfdto == null)
                return null;

            List<DisplayItem> Categories = new List<DisplayItem>();
            foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
            {
                Categories.Add(new DisplayItem
                {
                    Label = $"{FacetItem.Label}",
                    Url = $"{Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}" + (ShowResults ? "/1" : string.Empty),
                    Count = $"{FacetItem.Count}"
                });
            }
            return new BrowseJobsByTypeViewModel() { Items = Categories, Header = Header, HideAllLink = HideAllLink, Breadcrumbs = BreadcrumbViewModel };
        }

        #endregion

        #region Browse job by location helpers
        private void DeterminePaginationRange(ref BrowseJobsByTypeViewModel Model)
        {
            int? CurrentPage = Model.CurrentPage;
            int NumberOfPages = Model.NumberOfPages;

            // Base case when there are less than 5 pages of results returned
            if (NumberOfPages <= 3)
            {
                Model.PaginationRangeLow = 1;
                Model.PaginationRangeHigh = NumberOfPages;
                return;
            }

            // Base case when the current page is one of first two pages
            if (CurrentPage < 2)
            {
                Model.PaginationRangeLow = 1;
                Model.PaginationRangeHigh = 3;
                return;
            }

            // Base case for when current page is one of last two pages
            if (CurrentPage > (NumberOfPages - 1))
            {
                Model.PaginationRangeLow = NumberOfPages - 2;
                Model.PaginationRangeHigh = NumberOfPages;
                return;
            }

            // Last case for when current page is somewhere in the middle of a result set
            // of greater than 5 pages.
            Model.PaginationRangeLow = (int)CurrentPage - 1;
            Model.PaginationRangeHigh = (int)CurrentPage + 1;
        }


        private string AssembleBaseLocationUrl(
            string country = null,
            string state = null,
            string city = null,
            string industry = null,
            string category = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/browse-jobs-location" +
                (country == null ? string.Empty : "/" + country) +
                (state == null ? string.Empty : "/" + state) +
                (city == null ? string.Empty : "/" + city) +
                (industry == null ? string.Empty : "/" + industry) +
                (category == null ? string.Empty : "/" + category));
            return sb.ToString();
        }

        private string AssembleBaseIndustryUrl(
            string industry = null,
            string category = null,
            string country = null,
            string state = null,
            string city = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("/browse-jobs-industry" +

                (industry == null ? string.Empty : "/" + industry) +
                (category == null ? string.Empty : "/" + category) +
                (country == null ? string.Empty : "/" + country) +
                (state == null ? string.Empty : "/" + state) +
                (city == null ? string.Empty : "/" + city));
            return sb.ToString();
        }

        private bool IsValidUrl(
            JobSearchResultDto SearchResult,
            string country,
            string state,
            string city,
            string industry,
            string category)
        {
            if (!string.IsNullOrEmpty(country) && !country.ToLower().Equals("us"))
                return false;

            if (!string.IsNullOrEmpty(state))
            {
                JobQueryFacetDto StateFacet = FindNeededFacet("admin_1", SearchResult.Facets);
                if (StateFacet == null)
                    return false;

                if (state.Length == 2)
                {
                    if (!FacetLabelExists(StateFacet.Facets, state))
                        return false;
                }
                else
                {
                    try
                    {
                         
                        if (!FacetLabelExists(StateFacet.Facets, UpDiddyLib.Helpers.Utils.GetStateByName(state.Replace("-", " ")).ToString()))
                            return false;
                    }
                    catch (Exception)
                    {
                        // Returns false if unable to find matching state from
                        return false;
                    }
                }

            }

            if (!string.IsNullOrEmpty(city))
            {
                JobQueryFacetDto CityFacet = FindNeededFacet("city", SearchResult.Facets);
                if (CityFacet == null || !FacetLabelExists(CityFacet.Facets, city))
                    return false;
            }

            if (!string.IsNullOrEmpty(industry))
            {
                JobQueryFacetDto IndustryFacet = FindNeededFacet("industry", SearchResult.Facets);
                if (IndustryFacet == null)
                    return false;
            }

            if (!string.IsNullOrEmpty(category))
            {
                JobQueryFacetDto CategoryFacet = FindNeededFacet("jobcategory", SearchResult.Facets);
                if (CategoryFacet == null)
                    return false;
            }


            return true;

        }

        private bool FacetLabelExists(List<JobQueryFacetItemDto> List, string Label)
        {
            foreach (JobQueryFacetItemDto Item in List)
            {
                if (Regex.Replace(Item.Label.Split(",")[0].ToLower(), @"\.| |'", "-").Equals(Label.ToLower()))
                    return true;
            }
            return false;
        }

        private JobQueryFacetDto FindNeededFacet(string key, List<JobQueryFacetDto> List)
        {
            foreach (JobQueryFacetDto facet in List)
            {
                if (facet.Name.ToLower().Equals(key.ToLower()))
                    return facet;
            }

            return null;
        }

        private string StateCodeToFullName(string stateCode)
        {
            // The following uses the C# inline out function, which initializes
            // the variable for the local scope.
            Enum.TryParse(stateCode.ToUpper(), out UpDiddyLib.Helpers.Utils.State state);
            return UpDiddyLib.Helpers.Utils.GetState(state);
        }


        #endregion

        [HttpGet("browse-jobs-industry/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry?}")]
        [HttpGet("browse-jobs-industry/{industry}/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry}/{category?}/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry}/{category}/{country?}/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry}/{category}/{country}/{state?}/{page:int?}")]
        [HttpGet("browse-jobs-industry/{industry}/{category}/{country}/{state}/{city?}/{page:int?}")]
        public async Task<IActionResult> BrowseJobsByIndustryAsync(
           string industry,
           string category,
           string country,
           string state,
           string city,
           int page)
        {




            if (!string.IsNullOrEmpty(industry) &&
                !string.IsNullOrEmpty(category) &&
                !string.IsNullOrEmpty(country) &&
                !string.IsNullOrEmpty(state) &&
                !string.IsNullOrEmpty(city) &&
                page == 0)
                return RedirectPermanent($"{Request.Path}/1");

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _Api.GetJobsUsingRoute(
                    country,
                    state,
                    city,
                    industry?.Replace("-", "+"),
                    category?.Replace("-", "+"),
                    page);
            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }

            if (page > jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0) || page < 0)
                return NotFound();

            if (jobSearchResultDto == null || !IsValidUrl(jobSearchResultDto, country, state, city, industry, category))
                return NotFound();

            int pageCount = _configuration.GetValue<int>("Pagination:PageCount");

            BrowseJobsByTypeViewModel jobSearchViewModel = new BrowseJobsByTypeViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount),
                CurrentPage = page,
                NumberOfPages = jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0)
            };

            if (User.Identity.IsAuthenticated)
            {
                jobSearchViewModel.FavoritesMap = await _Api.JobFavoritesByJobGuidAsync(jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount).Select(job => job.JobPostingGuid).ToList());
            }

            // Google seems to be capping the number of results at 500, so we account for that here.
            if (jobSearchViewModel.NumberOfPages > 500)
                jobSearchViewModel.NumberOfPages = 500;

            DeterminePaginationRange(ref jobSearchViewModel);

            Uri RequestUri = new Uri(($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}"));

            BreadcrumbViewModel BreadcrumbViewModel = CreateBreadcrumbs(RequestUri, jobSearchResultDto);
            jobSearchViewModel.Breadcrumbs = BreadcrumbViewModel;

            // User has reached the end of the browse flow, so present results.
            if (!string.IsNullOrEmpty(city) || page != 0)
            {
                if (string.IsNullOrEmpty(industry))
                    jobSearchViewModel.Header = "All";
                else if (string.IsNullOrEmpty(category))
                    jobSearchViewModel.Header = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                else if (string.IsNullOrEmpty(state))
                    jobSearchViewModel.Header = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                else if (string.IsNullOrEmpty(city))
                {
                    string StateName = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                    jobSearchViewModel.Header = StateName.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateName)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateName);
                }
                else if (!string.IsNullOrEmpty(city))
                    jobSearchViewModel.Header = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                jobSearchViewModel.BaseUrl = AssembleBaseIndustryUrl(country, state, city, industry, category);
                return View("BrowseByType", jobSearchViewModel);
            }


            JobQueryFacetDto jqfdto = FindNeededFacet("industry", jobSearchResultDto.Facets);

            if (string.IsNullOrEmpty(industry))
            {

                List<DisplayItem> Industries = new List<DisplayItem>();


                if (jqfdto?.Facets == null)
                    return RedirectPermanent(Request.Path + "/1");


                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Industries.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label}",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                        Count = $"{FacetItem.Count}"
                    });
                }
                return View("BrowseByType", new BrowseJobsByTypeViewModel() { Items = Industries, Header = "Select Desired Industry:", Breadcrumbs = BreadcrumbViewModel });
            }

            string IndustryLabel = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

            if (string.IsNullOrEmpty(category))
            {
                jqfdto = FindNeededFacet("jobcategory", jobSearchResultDto.Facets);


                List<DisplayItem> Categories = new List<DisplayItem>();

                if (jqfdto?.Facets == null)
                    return RedirectPermanent(Request.Path + "/1");

                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Categories.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label}",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                        Count = $"{FacetItem.Count}"
                    });
                }

                return View("BrowseByType", new BrowseJobsByTypeViewModel() { Items = Categories, Header = IndustryLabel, Breadcrumbs = BreadcrumbViewModel });

            }

            string CategoryLabel = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;



            // Return state view if user has only specified country
            if (string.IsNullOrEmpty(state))
            {
                jqfdto = FindNeededFacet("admin_1", jobSearchResultDto.Facets);
                List<DisplayItem> StateLocations = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto JobQueryFacet in jqfdto.Facets)
                {
                    UpDiddyLib.Helpers.Utils.State State;
                    Enum.TryParse(JobQueryFacet.Label.ToUpper(), out State);
                    string StateName = UpDiddyLib.Helpers.Utils.GetState(State);
                    StateLocations.Add(new DisplayItem
                    {
                        Label = $"{UpDiddyLib.Helpers.Utils.ToTitleCase(StateName)}",
                        Url = $"{Request.Path}/us/{JobQueryFacet.Label.ToLower()}",
                        Count = $"{JobQueryFacet.Count}"
                    });
                }

                BrowseJobsByTypeViewModel bjlvmState = new BrowseJobsByTypeViewModel()
                {
                    Items = StateLocations,
                    Header = CategoryLabel,
                    Breadcrumbs = BreadcrumbViewModel
                };

                return View("BrowseByType", bjlvmState);
            }

            // If flow reaches this point, user has specified country and state, but 
            // needs to choose city


            string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
            StateLabel = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel);


            if (string.IsNullOrEmpty(city))
            {

                jqfdto = FindNeededFacet("city", jobSearchResultDto.Facets);

                // City histogram wasn't found
                if (jqfdto == null)
                    return RedirectPermanent(Request.Path + "/1");

                List<DisplayItem> LocationsCities = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Regex rgx = new Regex("[^a-zA-Z]");
                    LocationsCities.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label.Split(",")[0]}",
                        Url = $"{Request.Path}/{rgx.Replace(FacetItem.Label.Split(",")[0].ToLower(), "-")}/1",
                        Count = $"{FacetItem.Count}"
                    });
                }
                BrowseJobsByTypeViewModel bjlvm = new BrowseJobsByTypeViewModel()
                {
                    Items = LocationsCities,
                    Header = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel),
                    Breadcrumbs = BreadcrumbViewModel

                };

                return View("BrowseByType", bjlvm);
            }





            return NotFound();



        }

        [HttpGet("browse-jobs-category/{page:int?}")]
        [HttpGet("browse-jobs-category/{category?}")]
        [HttpGet("browse-jobs-category/{category}/{page:int?}")]
        [HttpGet("browse-jobs-category/{category}/{industry?}/{page:int?}")]
        [HttpGet("browse-jobs-category/{category}/{industry}/{country?}/{page:int?}")]
        [HttpGet("browse-jobs-category/{category}/{industry}/{country}/{state?}/{page:int?}")]
        [HttpGet("browse-jobs-category/{category}/{industry}/{country}/{state}/{city?}/{page:int?}")]
        public async Task<IActionResult> BrowseJobsByCategoryAsync(
           string category,
           string industry,
           string country,
           string state,
           string city,
           int page)
        {




            if (!string.IsNullOrEmpty(industry) &&
                !string.IsNullOrEmpty(category) &&
                !string.IsNullOrEmpty(country) &&
                !string.IsNullOrEmpty(state) &&
                !string.IsNullOrEmpty(city) &&
                page == 0)
                return RedirectPermanent($"{Request.Path}/1");

            JobSearchResultDto jobSearchResultDto = null;

            try
            {
                jobSearchResultDto = await _Api.GetJobsUsingRoute(
                    country,
                    state,
                    city,
                    industry?.Replace("-", "+"),
                    category?.Replace("-", "+"),
                    page);
            }
            catch (ApiException e)
            {
                switch (e.ResponseDto.StatusCode)
                {
                    case (401):
                        return Unauthorized();
                    case (500):
                        return StatusCode(500);
                    default:
                        return NotFound();
                }
            }

            if (page > jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0) || page < 0)
                return NotFound();

            if (jobSearchResultDto == null || !IsValidUrl(jobSearchResultDto, country, state, city, industry, category))
                return NotFound();

            int pageCount = _configuration.GetValue<int>("Pagination:PageCount");

            BrowseJobsByTypeViewModel jobSearchViewModel = new BrowseJobsByTypeViewModel()
            {
                RequestId = jobSearchResultDto.RequestId,
                ClientEventId = jobSearchResultDto.ClientEventId,
                JobsSearchResult = jobSearchResultDto.Jobs.ToPagedList(1, pageCount),
                CurrentPage = page,
                NumberOfPages = jobSearchResultDto.TotalHits / 10 + (((jobSearchResultDto.TotalHits % 10) > 0) ? 1 : 0)
            };

            if (User.Identity.IsAuthenticated)
            {
                jobSearchViewModel.FavoritesMap = await _Api.JobFavoritesByJobGuidAsync(jobSearchResultDto.Jobs.ToPagedList(page == 0 ? 1 : page, pageCount).Select(job => job.JobPostingGuid).ToList());
            }

            // Google seems to be capping the number of results at 500, so we account for that here.
            if (jobSearchViewModel.NumberOfPages > 500)
                jobSearchViewModel.NumberOfPages = 500;

            DeterminePaginationRange(ref jobSearchViewModel);

            Uri RequestUri = new Uri(($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}"));

            BreadcrumbViewModel BreadcrumbViewModel = CreateBreadcrumbs(RequestUri, jobSearchResultDto);
            jobSearchViewModel.Breadcrumbs = BreadcrumbViewModel;

            // User has reached the end of the browse flow, so present results.
            if (!string.IsNullOrEmpty(city) || page != 0)
            {
                if (string.IsNullOrEmpty(category))
                    jobSearchViewModel.Header = "All";
                else if (string.IsNullOrEmpty(industry))
                    jobSearchViewModel.Header = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                else if (string.IsNullOrEmpty(state))
                    jobSearchViewModel.Header = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                else if (string.IsNullOrEmpty(city))
                {
                    string StateName = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
                    jobSearchViewModel.Header = StateName.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateName)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateName);
                }
                else if (!string.IsNullOrEmpty(city))
                    jobSearchViewModel.Header = FindNeededFacet("city", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

                jobSearchViewModel.BaseUrl = AssembleBaseIndustryUrl(country, state, city, industry, category);
                return View("BrowseByType", jobSearchViewModel);
            }

            JobQueryFacetDto jqfdto = FindNeededFacet("jobcategory", jobSearchResultDto.Facets);


            if (string.IsNullOrEmpty(category))
            {



                List<DisplayItem> Categories = new List<DisplayItem>();

                if (jqfdto?.Facets == null)
                    return RedirectPermanent(Request.Path + "/1");

                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Categories.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label}",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                        Count = $"{FacetItem.Count}"
                    });
                }
                return View("BrowseByType", new BrowseJobsByTypeViewModel() { Items = Categories, Header = "Select Desired Category:", Breadcrumbs = BreadcrumbViewModel });

            }

            string CategoryLabel = FindNeededFacet("jobcategory", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;

            if (string.IsNullOrEmpty(industry))
            {
                jqfdto = FindNeededFacet("industry", jobSearchResultDto.Facets);
                List<DisplayItem> Industries = new List<DisplayItem>();


                if (jqfdto?.Facets == null)
                    return RedirectPermanent(Request.Path + "/1");


                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Industries.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label}",
                        Url = $"{Request.Path}/{FacetItem.Label.Replace(" ", "-").ToLower()}",
                        Count = $"{FacetItem.Count}"
                    });
                }

                return View("BrowseByType", new BrowseJobsByTypeViewModel() { Items = Industries, Header = CategoryLabel, Breadcrumbs = BreadcrumbViewModel });
            }

            string IndustryLabel = FindNeededFacet("industry", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;


            // Return state view if user has only specified country
            if (string.IsNullOrEmpty(state))
            {
                jqfdto = FindNeededFacet("admin_1", jobSearchResultDto.Facets);
                List<DisplayItem> StateLocations = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto JobQueryFacet in jqfdto.Facets)
                {
                    UpDiddyLib.Helpers.Utils.State State;
                    Enum.TryParse(JobQueryFacet.Label.ToUpper(), out State);
                    string StateName = UpDiddyLib.Helpers.Utils.GetState(State);
                    StateLocations.Add(new DisplayItem
                    {
                        Label = $"{UpDiddyLib.Helpers.Utils.ToTitleCase(StateName)}",
                        Url = $"{Request.Path}/us/{JobQueryFacet.Label.ToLower()}",
                        Count = $"{JobQueryFacet.Count}"
                    });
                }

                BrowseJobsByTypeViewModel bjlvmState = new BrowseJobsByTypeViewModel()
                {
                    Items = StateLocations,
                    Header = IndustryLabel,
                    Breadcrumbs = BreadcrumbViewModel
                };

                return View("BrowseByType", bjlvmState);
            }


            string StateLabel = FindNeededFacet("admin_1", jobSearchResultDto.Facets).Facets.FirstOrDefault().Label;
            StateLabel = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel);


            // If flow reaches this point, user has specified country and state, but 
            // needs to choose city
            if (string.IsNullOrEmpty(city))
            {

                jqfdto = FindNeededFacet("city", jobSearchResultDto.Facets);

                // City histogram wasn't found
                if (jqfdto == null)
                    return RedirectPermanent(Request.Path + "/1");

                List<DisplayItem> LocationsCities = new List<DisplayItem>();
                jqfdto.Facets.Sort((x, y) => string.Compare(x.Label, y.Label));
                foreach (JobQueryFacetItemDto FacetItem in jqfdto.Facets)
                {
                    Regex rgx = new Regex("[^a-zA-Z]");
                    LocationsCities.Add(new DisplayItem
                    {
                        Label = $"{FacetItem.Label.Split(",")[0]}",
                        Url = $"{Request.Path}/{rgx.Replace(FacetItem.Label.Split(",")[0].ToLower(), "-")}/1",
                        Count = $"{FacetItem.Count}"
                    });
                }

                BrowseJobsByTypeViewModel bjlvm = new BrowseJobsByTypeViewModel()
                {
                    Items = LocationsCities,
                    Header = StateLabel.Length == 2 ? UpDiddyLib.Helpers.Utils.ToTitleCase(StateCodeToFullName(StateLabel)) : UpDiddyLib.Helpers.Utils.ToTitleCase(StateLabel),
                    Breadcrumbs = BreadcrumbViewModel
                };

                return View("BrowseByType", bjlvm);
            }





            return NotFound();



        }

        [Authorize]
        [HttpPost]
        [Route("[controller]/ReferAJob", Name = "ReferJobToFriend")]
        public async Task<IActionResult> ReferAJob(string jobPostingId, string referrerGuid, string refereeName, string refereeEmailId, string descriptionEmailBody)
        {
            //send email to referree for the job posting
            await _Api.ReferJobPosting(jobPostingId, referrerGuid, refereeName, refereeEmailId, descriptionEmailBody);

            Guid jobPostingGuid = Guid.Parse(jobPostingId);
            return await JobAsync(jobPostingGuid);
        }

        #region Keyword and location Search
        [HttpGet]
        [Route("[controller]/SearchKeyword")]
        public async Task<IActionResult> KeywordSearch(string value)
        {
            var keywordSearchList = await _Api.GetKeywordSearchTermsAsync(value);

            // consider modifying the front end to include the type information next to each intellisense result
            // it will be easy to support this by returning the keywordSearchList instead of valuesOnly
            // https://jqueryui.com/autocomplete/#categories
            var valuesOnly = keywordSearchList.Select(k => k.Value).ToList();

            return Ok(valuesOnly);
        }

        [HttpGet]
        [Route("[controller]/LocationKeyword")]
        public async Task<IActionResult> LocationSearch(string value)
        {
            var locationSearchList = await _Api.GetLocationSearchTermsAsync(value);

            // consider modifying the front end to include the type information next to each intellisense result
            // it will be easy to support this by returning the locationSearchList instead of valuesOnly
            // https://jqueryui.com/autocomplete/#categories
            var valuesOnly = locationSearchList.Select(l => l.Value).ToList();

            return Ok(valuesOnly);
        }
        #endregion

    */
    }
}
