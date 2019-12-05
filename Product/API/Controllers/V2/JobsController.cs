﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using System.Security.Claims;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Shared.GoogleJobs;
using System.Collections.Generic;

namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
    public class JobsController : BaseApiController
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30;
        private readonly ICloudTalentService _cloudTalentService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IServiceProvider _services;
        private readonly IJobService _jobService;
        private readonly IHangfireService _hangfireService;
        private readonly ISubscriberService _subscriberService;
        private readonly IJobPostingService _jobPostingService;
        private readonly IJobAlertService _jobAlertService;
        private readonly IJobFavoriteService _jobFavoriteService;
        private readonly IJobSearchService _jobSearchService;
        private readonly ITrackingService _trackingService;
        private readonly IJobApplicationService _jobApplicationService;
        private readonly IKeywordService _keywordService;

        #region constructor 
        public JobsController(IServiceProvider services
        , IHangfireService hangfireService
        , IJobAlertService jobAlertService
        , IJobFavoriteService jobFavoriteService
        , IJobSearchService jobSearchService
        , ICloudTalentService cloudTalentService
        , ITrackingService trackingService
        , IKeywordService keywordService)

        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _subscriberService = _services.GetService<ISubscriberService>();
            _jobApplicationService = _services.GetService<IJobApplicationService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalentService = cloudTalentService;

            //job Service to perform all business logic related to jobs
            _jobService = _services.GetService<IJobService>();
            _jobPostingService = _services.GetService<IJobPostingService>();
            _hangfireService = hangfireService;
            _jobAlertService = jobAlertService;
            _jobFavoriteService = jobFavoriteService;
            _jobSearchService = jobSearchService;
            _keywordService = keywordService;
        }

        #endregion


        #region CloudTalentTracking

        [HttpPost]
        [Route("{job:guid}/tracking/{requestId}/{clientEventId}")]
        [Authorize]
        public async Task<IActionResult> TrackClientEventJobViewAction(Guid job, string requestId, string clientEventId)
        {
            await _cloudTalentService.TrackClientEventJobViewAction(job, requestId, clientEventId);
            return StatusCode(202);
        }
        #endregion

        #region Job Applications


        [HttpPost]
        [Route("{JobGuid:guid}/applications")]
        [Authorize]
        public async Task<IActionResult> CreateJobApplication([FromBody] ApplicationDto jobApplicationDto, Guid JobGuid)
        {

            await _jobApplicationService.CreateJobApplication(GetSubscriberGuid(), JobGuid, jobApplicationDto);
            return StatusCode(201);
        }


        #endregion

        #region Job Browse 


        [HttpGet]
        [Route("browse-location")]
        public async Task<IActionResult> BrowseJobsByLocation()
        {
            JobBrowseResultDto rVal = null;
            rVal = await _jobService.BrowseJobsByLocation(Request.Query);
            return Ok(rVal);
        }

        #endregion

        #region Job Search

        [HttpGet]
        [Route("search/{JobGuid:guid}")]
        public async Task<IActionResult> GetJob(Guid JobGuid)
        {

            JobDetailDto rVal = await _jobService.GetJobDetail(JobGuid);
            return Ok(rVal);
        }

        [HttpGet]
        [Route("search/keyword")]
        public async Task<IActionResult> GetKeywordSearchTerms(string value)
        {            
            var rVal = await _keywordService.GetKeywordSearchTerms(value);
            return Ok(rVal);
        }


        [HttpGet]
        [Route("search/location")]
        public async Task<IActionResult> GetLocationSearchTerms(string value)
        {
            var rVal = await _keywordService.GetLocationSearchTerms(value);
            return Ok(rVal);
        }


        [HttpPost]
        [Route("{job:guid}/share")]
        [Authorize]
        public async Task<IActionResult> Share([FromBody] ShareJobDto shareJobDto, Guid job)
        {
            await _jobService.ShareJob(GetSubscriberGuid(), job, shareJobDto);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("search")]
        public async Task<ActionResult> Search()
        {
            JobSearchSummaryResultDto rVal = await _jobService.SummaryJobSearch(Request.Query);
            return Ok(rVal);
        }

        [HttpGet]
        [Route("search/count")]
        public async Task<IActionResult> GetActiveJobCount()
        {
            var count = await _jobSearchService.GetActiveJobCount();
            return Ok(count);
        }

        [HttpGet]
        [Route("search/{job:guid}/similar")]
        public async Task<IActionResult> GetSimilarJobs(Guid job)
        {
            var jobs = await _jobSearchService.GetSimilarJobs(job);
            return Ok(jobs);
        }

        [HttpGet]
        [Route("search/state-map")]
        public async Task<IActionResult> GetStateMapData()
        {
            var stateMapdto = await _jobSearchService.GetStateMapData();
            return Ok(stateMapdto);
        }


        #endregion

        #region Job Alert

        [HttpPost]
        [Route("alert")]
        [Authorize]
        public async Task<IActionResult> CreateJobAlert([FromBody] JobAlertDto jobPostingAlertDto)
        {

            await _jobAlertService.CreateJobAlert(GetSubscriberGuid(), jobPostingAlertDto);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("alert")]
        [Authorize]
        public async Task<IActionResult> GetJobAlerts()
        {

            var jobAlerts = await _jobAlertService.GetJobAlert(GetSubscriberGuid());
            return Ok(jobAlerts);
        }

        [HttpPut]
        [Route("alert/{jobAlert:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateJobAlert([FromBody] JobAlertDto jobPostingAlertDto, Guid jobAlert)
        {

            await _jobAlertService.UpdateJobAlert(GetSubscriberGuid(), jobAlert, jobPostingAlertDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("alert/{jobAlert:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteJobAlert(Guid jobAlert)
        {

            await _jobAlertService.DeleteJobAlert(GetSubscriberGuid(), jobAlert);
            return StatusCode(204);
        }

        #endregion

        #region Job Favorites

        [HttpPut]
        [Route("{job:guid}/favorites")]
        [Authorize]
        public async Task<IActionResult> CreateJobFavorite(Guid job)
        {

            await _jobFavoriteService.AddJobToFavorite(GetSubscriberGuid(), job);
            return StatusCode(201);
        }

        [HttpGet]
        [Route("favorites")]
        [Authorize]
        public async Task<IActionResult> GetJobFavorites()
        {

            var favorites = await _jobFavoriteService.GetJobFavorites(GetSubscriberGuid());
            return Ok(favorites);
        }

        [HttpDelete]
        [Route("{job:guid}/favorites")]
        [Authorize]
        public async Task<IActionResult> DeleteJobFavorite(Guid job)
        {

            await _jobFavoriteService.DeleteJobFavorite(GetSubscriberGuid(), job);
            return StatusCode(204);
        }

        #endregion


        #region Job crud



        [HttpPost]
        [Route("admin")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> CreateJob([FromBody] UpDiddyLib.Dto.JobPostingDto jobPostingDto)
        {

            await _jobPostingService.CreateJobPosting(GetSubscriberGuid(), jobPostingDto);
            return StatusCode(201);
        }




        [HttpPut]
        [Route("admin/{jobGuid:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> UpdateJob([FromBody] UpDiddyLib.Dto.JobPostingDto jobPostingDto, Guid jobGuid)
        {

            await _jobPostingService.UpdateJobPosting(GetSubscriberGuid(), jobGuid,jobPostingDto);
            return StatusCode(204);
        }



        [HttpDelete]
        [Route("admin/{jobGuid:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> DeleteJob([FromBody] UpDiddyLib.Dto.JobPostingDto jobPostingDto, Guid jobGuid)
        {

            await _jobPostingService.DeleteJobPosting(GetSubscriberGuid(), jobGuid );
            return StatusCode(204);
        }

        [HttpGet]
        [Route("admin/{jobGuid:guid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetJobAdmin(Guid jobGuid)
        {

            UpDiddyLib.Dto.JobPostingDto jobPostingDto =  await _jobPostingService.GetJobPosting(GetSubscriberGuid(), jobGuid);
            return Ok(jobPostingDto);
        }

        [HttpGet]
        [Route("admin")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetJobAdminForSubscriber(Guid jobGuid)
        {

            List<UpDiddyLib.Dto.JobPostingDto> postings = await _jobPostingService.GetJobPostingForSubscriber(GetSubscriberGuid());
            return Ok(postings);
        }






        #endregion

    }
}