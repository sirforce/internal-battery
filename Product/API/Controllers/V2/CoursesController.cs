﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore;
using UpDiddyLib.Helpers;
using System.Net.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Dto;


namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
    public class CoursesController : BaseApiController
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        private WozInterface _wozInterface = null;
        protected readonly ILogger _syslog = null;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ISysEmail _sysemail;
        private readonly IDistributedCache _distributedCache;
        private readonly IHangfireService _hangfireService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ICourseService _courseService;
        private readonly ICourseFavoriteService _courseFavoriteService;
        private readonly ICourseEnrollmentService _courseEnrollmentService;
        private readonly IPromoCodeService _promoCodeService;
        private readonly ISkillService _skillService;

        public CoursesController(UpDiddyDbContext db
        , IMapper mapper
        , IConfiguration configuration
        , ISysEmail sysemail
        , IHttpClientFactory httpClientFactory
        , ILogger<CourseController> syslog
        , IDistributedCache distributedCache
        , IHangfireService hangfireService
        , IRepositoryWrapper repositoryWrapper
        , ICourseService courseService
        , ISkillService skillService
        , ICourseFavoriteService courseFavoriteService
        , ICourseEnrollmentService courseEnrollmentService
        , IPromoCodeService promoCodeService)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _syslog = syslog;
            _httpClientFactory = httpClientFactory;
            _wozInterface = new WozInterface(_db, _mapper, _configuration, _syslog, _httpClientFactory);
            _sysemail = sysemail;
            _distributedCache = distributedCache;
            _repositoryWrapper = repositoryWrapper;
            _courseService = courseService;
            _courseFavoriteService = courseFavoriteService;
            _courseEnrollmentService = courseEnrollmentService;
            _promoCodeService = promoCodeService;
            _skillService = skillService;
        }

        #region course metrics 

        [HttpGet]
        [Route("count")]
        public async Task<IActionResult> GetCoursesCount()
        {
            var count = await _courseService.GetCoursesCount();
            return Ok(count);
        }

        #endregion

        #region Course Variants 

        [HttpGet]
        [Route("{course:guid}/variant")]
        public async Task<IActionResult> GetCoursVariants(Guid course)
        {
            var variants = await _courseService.GetCourseVariants(course);
            return Ok(variants);
        }

        #endregion

        #region CourseDetails

        [HttpGet]
        [Route("topic/{topicGuid:guid}")]
        public async Task<IActionResult> GetCoursesByTopic(Guid topicGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var courses = await _courseService.GetCoursesByTopic(topicGuid, limit, offset, sort, order);
            return Ok(courses);
        }

        [HttpGet]
        [Route("{course:guid}")]
        public async Task<IActionResult> GetCourse(Guid course)
        {
            var courses = await _courseService.GetCourse(course);
            return Ok(courses);
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var courses = await _courseService.GetCourses(limit, offset, sort, order);
            return Ok(courses);
        }

        [HttpGet]
        [Route("random")]
        public async Task<IActionResult> Random()
        {
            return Ok(await _courseService.GetCoursesRandom(Request.Query));
        }

        #endregion

        #region Course Enrollments


        [HttpGet]
        [Authorize]
        [Route("{courseGuid}/variant/{courseVariant}/promocodes/{promoCode}")]
        public async Task<IActionResult> GetCoursesEnrollmentInfo(Guid courseGuid, Guid courseVariant, string promoCode)
        {
            PromoCodeDto rVal = _promoCodeService.GetPromoCode(GetSubscriberGuid(), promoCode, courseVariant);
            return Ok(rVal);
        }


        [HttpGet]
        [Authorize]
        [Route("{courseGuid}/enroll")]
        public async Task<IActionResult> GetCoursesEnrollmentInfo(Guid courseGuid)
        {
            CourseCheckoutInfoDto rVal = await _courseEnrollmentService.GetCourseCheckoutInfo(GetSubscriberGuid(), courseGuid);
            return Ok(rVal);
        }

        [HttpPost]
        [Authorize]
        [Route("{courseGuid}/enroll")]
        public async Task<IActionResult> EnrollSubscriber([FromBody] CourseEnrollmentDto courseEnrollmentDto, Guid courseGuid)
        {

            var enrollmentGuid = await _courseEnrollmentService.Enroll(GetSubscriberGuid(), courseEnrollmentDto, courseGuid);
            return StatusCode(201, enrollmentGuid);
        }

        #endregion

        #region Course Query
        [HttpGet]
        [Route("query")]
        public async Task<IActionResult> SearchCourses(int limit = 10, int offset = 0, string sort = "ModifyDate", string order = "descending", string keyword = "*", string level = "", string topic = "")
        {
            var rVal = await _courseService.SearchCoursesAsync(limit, offset, sort, order, keyword, level, topic);
            return Ok(rVal);
        }

        #endregion 

        #region Course Favorites 

        [HttpGet]
        [Route("favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavoriteCourses(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var isFavorite = await _courseFavoriteService.GetFavoriteCourses(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(isFavorite);
        }

        [HttpGet]
        [Route("{course}/favorites")]
        [Authorize]
        public async Task<IActionResult> IsCourseAddedToFavorite(Guid course)
        {
            var isFavorite = await _courseFavoriteService.IsCourseAddedToFavorite(GetSubscriberGuid(), course);
            return Ok(isFavorite);
        }

        [HttpPost]
        [Route("{course:guid}/favorites")]
        [Authorize]
        public async Task<IActionResult> AddCourseFavorite(Guid course)
        {
            var courseFavoriteGuid = await _courseFavoriteService.AddToFavorite(GetSubscriberGuid(), course);
            return StatusCode(201, courseFavoriteGuid);
        }

        [HttpDelete]
        [Route("{course:guid}/favorites")]
        [Authorize]
        public async Task<IActionResult> RemoveCourseFavorite(Guid course)
        {
            await _courseFavoriteService.RemoveFromFavorite(GetSubscriberGuid(), course);
            return StatusCode(204);
        }

        #endregion

        #region Related Entities

        [HttpPost]
        [Route("courses/related")]
        public async Task<IActionResult> GetRelatedCoursesByCourses([FromBody] List<Guid> courses, int limit = 100, int offset = 0)
        {
            List<RelatedCourseDto> relatedCourses = null;
            relatedCourses = await _courseService.GetCoursesByCourses(courses, limit, offset);
            return StatusCode(200, relatedCourses);
        }

        [HttpGet]
        [Route("courses/{course:guid}/related")]
        public async Task<IActionResult> GetRelatedCoursesByCourse(Guid course, int limit = 100, int offset = 0)
        {
            List<RelatedCourseDto> relatedCourses = null;
            relatedCourses = await _courseService.GetCoursesByCourse(course, limit, offset);
            return StatusCode(200, relatedCourses);
        }

        [HttpPost]
        [Route("jobs/related")]
        public async Task<IActionResult> GetRelatedCoursesByJobs([FromBody] List<Guid> jobs, int limit = 100, int offset = 0)
        {
            List<RelatedCourseDto> relatedCourses = null;
            relatedCourses = await _courseService.GetCoursesByJobs(jobs, limit, offset);
            return StatusCode(200, relatedCourses);
        }

        [HttpGet]
        [Route("jobs/{job:guid}/related")]
        public async Task<IActionResult> GetRelatedCoursesByJob(Guid job, int limit = 100, int offset = 0)
        {
            List<RelatedCourseDto> relatedCourses = null;
            relatedCourses = await _courseService.GetCoursesByJob(job, limit, offset);
            return StatusCode(200, relatedCourses);
        }

        [HttpGet]
        [Route("subscribers/related")]
        public async Task<IActionResult> GetRelatedCoursesForSubscriber(int limit = 100, int offset = 0)
        {
            var subscriber = GetSubscriberGuid();
            if (subscriber == Guid.Empty)
                throw new NotFoundException("Subscriber not found");
            var relatedCourses = await _courseService.GetCoursesBySubscriber(subscriber, limit, offset);
            return StatusCode(200, relatedCourses);
        }

        #endregion

        #region Refer A Friend

        [HttpPost]
        [Authorize]
        [Route("refer")]
        public async Task<IActionResult> ReferAFriend([FromBody] CourseReferralDto courseReferral)
        {
            var courseReferralGuid = await _courseService.ReferCourseToFriend(GetSubscriberGuid(), courseReferral);
            return StatusCode(201, courseReferralGuid);
        }

        #endregion

        #region Skills

        [HttpGet]
        [Route("{course:guid}/skills")]
        public async Task<IActionResult> GetSkillForCourse(Guid course)
        {
            var result = await _skillService.GetSkillsByCourseGuid(course);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("{course:guid}/skills")]
        public async Task<IActionResult> UpdateCourseSkills(Guid course, [FromBody] List<Guid> skills)
        {
            await _skillService.UpdateCourseSkills(course, skills);
            return StatusCode(200);
        }

        #endregion
    }
}