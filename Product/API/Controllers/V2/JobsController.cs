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
namespace UpDiddyApi.Controllers
{

    [ApiController]
    public class JobsController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30;
        private readonly CloudTalent _cloudTalent = null;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IServiceProvider _services;
        private readonly IJobService _jobService;
        private readonly IHangfireService _hangfireService;
        private readonly ISubscriberService _subscriberService;
        private readonly IJobPostingService _jobPostingService;
        private readonly IJobAlertService _jobAlertService;

        #region constructor 
        public JobsController(IServiceProvider services, IHangfireService hangfireService)
 

        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _subscriberService = _services.GetService<ISubscriberService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);

            //job Service to perform all business logic related to jobs
            _jobService = _services.GetService<IJobService>();
            _jobPostingService = _services.GetService<IJobPostingService>();
            _hangfireService = hangfireService;
         //   _jobAlertService = jobAlertService;
        }

        #endregion

        #region Job Search
        [HttpGet]
        [Route("/V2/[controller]/search/{JobGuid}")]
        public async Task<IActionResult> GetJob(Guid JobGuid)
        {
            try
            {
                JobDetailDto rVal = await _jobService.GetJobDetail(JobGuid);
                return Ok(rVal);
            }
            catch (NotFoundException e)
            {
                return NotFound();
            }
            catch 
            {
                return BadRequest();
            }        
        }


        [HttpGet]
        [Route("/V2/[controller]/search")]
        public async Task<IActionResult> SearchJobs()
        {
            JobSearchSummaryResultDto rVal = null;
            rVal = await _jobService.SummaryJobSearch(Request.Query);
            return Ok(rVal);
        }

        #endregion

        #region Job Alert

        [HttpPost]
        [Route("/V2/[controller]/alert")]
        [Authorize]
        public async Task<IActionResult> CreateJobAlert([FromBody] JobAlertDto jobPostingAlertDto)
        {
            try
            {
                Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await _jobAlertService.CreateJobAlert(subscriberGuid, jobPostingAlertDto);
                return StatusCode(201);
            }
            catch (MaximumReachedException e)
            {
                return BadRequest(e);
            }
            catch (Exception e)
            {

            }
            return Ok();
        }

        [HttpGet]
        [Route("/V2/[controller]/alert")]
        [Authorize]
        public async Task<IActionResult> GetJobAlerts()
        {
            try
            {
                Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var jobAlerts = await _jobAlertService.GetJobAlert(subscriberGuid);
                return Ok(jobAlerts);
            }
            catch (NotFoundException e)
            {
                return BadRequest(e);
            }
            catch (Exception e)
            {

            }

            return Ok();
        }

        [HttpDelete]
        [Route("/V2/[controller]/alert")]
        [Authorize]
        public async Task<IActionResult> DeleteJobAlert(Guid jobAlertGuid)
        {
            try
            {
                Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
             //   await _jobAlertService.DeleteJobAlert(jobAlertGuid);
                return StatusCode(204);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (NotFoundException e)
            {
                return BadRequest(e);
            }
            catch (Exception e)
            {

            }

            return Ok();
        }

        #endregion

    }
}