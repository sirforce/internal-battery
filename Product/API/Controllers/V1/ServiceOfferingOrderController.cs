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

        [HttpPost]
        [AllowAnonymous]
        [Route("api/[controller]")]
        public IActionResult CreateOrder( [FromBody] ServiceOfferingTransactionDto serviceOfferingTransactionDto)
        {
            Guid subscriberGuid = Guid.Empty;                       

            string Msg = "Order processed";
            int statusCode = 200;
            if (serviceOfferingTransactionDto?.CreateUserDto?.SubscriberGuid != null && serviceOfferingTransactionDto?.CreateUserDto?.SubscriberGuid != Guid.Empty)
                subscriberGuid = serviceOfferingTransactionDto.CreateUserDto.SubscriberGuid;
            else if (serviceOfferingTransactionDto?.ServiceOfferingOrderDto?.Subscriber?.SubscriberGuid != null && serviceOfferingTransactionDto?.ServiceOfferingOrderDto?.Subscriber?.SubscriberGuid != Guid.Empty)
                subscriberGuid = serviceOfferingTransactionDto.ServiceOfferingOrderDto.Subscriber.SubscriberGuid.Value;
            _serviceOfferingOrderService.ProcessOrder(serviceOfferingTransactionDto, subscriberGuid, ref statusCode, ref Msg);
            return Ok(new BasicResponseDto() { StatusCode = statusCode, Description = Msg });
		
        }

        [HttpGet("api/[controller]/subscriber-order/{orderGuid}")]
        public async Task<IActionResult> GetSubscriberOrder(Guid orderGuid){
            ServiceOfferingOrderDto serviceOfferingOrderDto = await _serviceOfferingOrderService.GetSubscriberOrder(orderGuid);
            if(serviceOfferingOrderDto == null)
                return NotFound();
            return Ok(serviceOfferingOrderDto);
        }
    }
}