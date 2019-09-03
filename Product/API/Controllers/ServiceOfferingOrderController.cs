﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Services;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using AutoMapper;
using UpDiddyApi.Helpers.Job;
using System.Security.Claims;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Http;
using UpDiddyApi.ApplicationCore.Interfaces;
namespace UpDiddyApi.Controllers
{
 
    [ApiController]
    public class ServiceOfferingOrderController : ControllerBase
    {

        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly int _postingTTL = 30;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IServiceProvider _services;
        private readonly IServiceOfferingOrderService _serviceOfferingOrderService;
 

        public ServiceOfferingOrderController(IServiceProvider services, IHangfireService hangfireService)

        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _mapper = _services.GetService<IMapper>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _syslog = _services.GetService<ILogger<JobController>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _serviceOfferingOrderService = _services.GetService<IServiceOfferingOrderService>();
            _postingTTL = int.Parse(_configuration["JobPosting:PostingTTLInDays"]);
     
        }

		//TODO JAB Authorize 
        [HttpPost]
        [Authorize]
        [Route("api/[controller]")]
        public IActionResult CreateOrder( [FromBody] ServiceOfferingOrderDto serviceOfferingOrderDto)
        {
            Guid subscriberGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            string Msg = "Order processed";
            int statusCode = 200;
            if (!_serviceOfferingOrderService.ProcessOrder(serviceOfferingOrderDto, subscriberGuid, ref statusCode, ref Msg))
                return BadRequest(new BasicResponseDto() { StatusCode = statusCode, Description = Msg });
			else
                return Ok(new BasicResponseDto() { StatusCode = statusCode, Description = Msg });
        }

    }
}