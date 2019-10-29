﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore;
using UpDiddyLib.Helpers;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hangfire;
using UpDiddyApi.Workflow;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
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


        public CoursesController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, ILogger<CourseController> syslog, IDistributedCache distributedCache, IHangfireService hangfireService, IRepositoryWrapper repositoryWrapper, ICourseService courseService)
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
        }



        [HttpGet]
        [Route("/V2/[controller]/random")]
        public async Task<IActionResult> Random(Guid JobGuid)
        {
            return Ok(await _courseService.GetCoursesRandom(Request.Query));
        }



        [HttpGet]
        [Route("/V2/[controller]/job/{JobGuid}/related")]
        public async Task<IActionResult> Search(Guid JobGuid)
        {
            return Ok(await _courseService.GetCoursesForJob(JobGuid,Request.Query)  );
        }



        [HttpPost]
        [Route("/V2/[controller]/skill/related")]
        public async Task<IActionResult> SearchBySkills([FromBody] Dictionary<string,int> SkillHistogram)
        {       
           return Ok(await _courseService.GetCoursesBySkillHistogram(SkillHistogram, Request.Query));
        }



    }
}