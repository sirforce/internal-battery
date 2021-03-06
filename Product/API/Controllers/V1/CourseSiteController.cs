﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers
{
    [ApiController]
    public class CourseSiteController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ICourseCrawlingService _courseCrawlingService;

        public CourseSiteController(ILogger<RecruiterController> logger, ICourseCrawlingService courseCrawlingService)
        {
            _logger = logger;
            _courseCrawlingService = courseCrawlingService;
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpGet]
        [Route("api/course-sites")]
        public async Task<IActionResult> CourseSitesAsync()
        {
            try
            {
                var courseSites = await _courseCrawlingService.GetCourseSitesAsync();
                return Ok(courseSites);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"CourseSiteController.CourseSitesAsync : Error occured when retrieving course sites: {ex.Message}", ex);
                return StatusCode(500);
            }
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpPatch]
        [Route("api/course-sites/{courseSiteGuid}/crawl")]
        public async Task<IActionResult> CrawlCourseSitesAsync(Guid courseSiteGuid)
        {
            try
            {
                var courseSiteDto = await _courseCrawlingService.CrawlCourseSiteAsync(courseSiteGuid);
                return Ok(courseSiteDto);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"CourseSiteController.CrawlCourseSitesAsync : Error occured when crawling course site: {ex.Message}", ex);
                return StatusCode(500);
            }
        }

        [Authorize(Policy = "IsCareerCircleAdmin")]
        [HttpPatch]
        [Route("api/course-sites/{courseSiteGuid}/sync")]
        public async Task<IActionResult> SyncCourseSitesAsync(Guid courseSiteGuid)
        {
            try
            {
                var courseSiteDto = await _courseCrawlingService.SyncCourseSiteAsync(courseSiteGuid);
                return Ok(courseSiteDto);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"CourseSiteController.SyncCourseSitesAsync : Error occured when syncing course site: {ex.Message}", ex);
                return StatusCode(500);
            }
        }
    }
}