﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using Microsoft.Extensions.DependencyInjection;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.Helpers.Job;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Google.Apis.CloudTalentSolution.v3.Data;
using UpDiddyLib.Shared.GoogleJobs;
using Microsoft.AspNetCore.Http;


namespace UpDiddyApi.ApplicationCore.Services
{
    public class ServiceOfferingService : IServiceOfferingService 
    {
        private readonly IServiceProvider _services;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private ISysEmail _sysEmail;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private IHangfireService _hangfireService;
        private readonly CloudTalent _cloudTalent = null;
        private readonly UpDiddyDbContext _db = null;
        private readonly ILogger _syslog;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ICompanyService _companyService;
        private readonly ISubscriberService _subscriberService;

        public ServiceOfferingService(IServiceProvider services, IHangfireService hangfireService)
        {
            _services = services;

            _db = _services.GetService<UpDiddyDbContext>();
            _syslog = _services.GetService<ILogger<JobService>>();
            _httpClientFactory = _services.GetService<IHttpClientFactory>();
            _repositoryWrapper = _services.GetService<IRepositoryWrapper>();
            _mapper = _services.GetService<IMapper>();
            _sysEmail = _services.GetService<ISysEmail>();
            _configuration = _services.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            _companyService = services.GetService<ICompanyService>();
            _subscriberService = services.GetService<ISubscriberService>();
            _hangfireService = hangfireService;
            _cloudTalent = new CloudTalent(_db, _mapper, _configuration, _syslog, _httpClientFactory, _repositoryWrapper, _subscriberService);
        }

        public async Task<IList<ServiceOfferingDto>> GetAllServiceOfferings()
        {
             
                IList<ServiceOffering> serviceOfferings = _repositoryWrapper.ServiceOfferingRepository.GetAllServiceOfferings();
                var rVal = _mapper.Map<IList<ServiceOfferingDto>>(serviceOfferings);
                return rVal;                   
        }

    }
}
