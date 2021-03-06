﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.Models;
using UpDiddyApi.Workflow;
using UpDiddyLib.Dto;
using System.Web;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Exceptions;
using AutoMapper.QueryableExtensions;

namespace UpDiddyApi.ApplicationCore.Services    
{
    public class SubscriberEducationalHistoryService : ISubscriberEducationalHistoryService
    {
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private IB2CGraph _graphClient { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper;
        private ITaggingService _taggingService { get; set; }
        private IHangfireService _hangfireService { get; set; }

        public SubscriberEducationalHistoryService(UpDiddyDbContext context,
            IConfiguration configuration,
            ICloudStorage cloudStorage,
            IB2CGraph graphClient,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,
            ITaggingService taggingService,
            IHangfireService hangfireService)
        {
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _graphClient = graphClient;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _taggingService = taggingService;
            _hangfireService = hangfireService;
        }

 
        public async Task<Guid> CreateEducationalHistory(SubscriberEducationHistoryDto EducationHistoryDto, Guid subscriberGuid)
        {
            // sanitize user inputs
            EducationHistoryDto.EducationalDegree = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalDegree);
            EducationHistoryDto.EducationalInstitution = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalInstitution);
 
            Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repository, subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} does not exist");
            // Find or create the institution 
            EducationalInstitution educationalInstitution = await EducationalInstitutionFactory.GetOrAdd(_repository, EducationHistoryDto.EducationalInstitution);
            int educationalInstitutionId = educationalInstitution.EducationalInstitutionId;
            // Find or create the degree major 
            EducationalDegree educationalDegree = await EducationalDegreeFactory.GetOrAdd(_repository, EducationHistoryDto.EducationalDegree);
            int educationalDegreeId = educationalDegree.EducationalDegreeId;
            // Find or create the degree type 
            EducationalDegreeType educationalDegreeType = await _repository.EducationalDegreeTypeRepository.GetByGuid(EducationHistoryDto.EducationalDegreeTypeGuid.Value);
            int educationalDegreeTypeId = 0;
            if (educationalDegreeType == null)
                educationalDegreeType = await EducationalDegreeTypeFactory.GetOrAdd(_repository, UpDiddyLib.Helpers.Constants.NotSpecifedOption);
            educationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId;

            SubscriberEducationHistory EducationHistory = new SubscriberEducationHistory()
            {
                SubscriberEducationHistoryGuid = Guid.NewGuid(),
                CreateGuid = Guid.Empty,
                ModifyGuid = Guid.Empty,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                IsDeleted = 0,
                SubscriberId = subscriber.SubscriberId,
                StartDate = EducationHistoryDto.StartDate,
                EndDate = EducationHistoryDto.EndDate,
                DegreeDate = EducationHistoryDto.DegreeDate,
                EducationalDegreeId = educationalDegreeId,
                EducationalDegreeTypeId = educationalDegreeTypeId,
                EducationalInstitutionId = educationalInstitutionId
            };

            _db.SubscriberEducationHistory.Add(EducationHistory);
            _db.SaveChanges();

            // update google profile 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));

            return EducationHistory.SubscriberEducationHistoryGuid;
        }

        public async Task<bool> UpdateEducationalHistory(SubscriberEducationHistoryDto EducationHistoryDto, Guid subscriberGuid, Guid educationalHistoryGuid)
        {

            EducationHistoryDto.EducationalDegree = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalDegree);
            EducationHistoryDto.EducationalInstitution = HttpUtility.HtmlEncode(EducationHistoryDto.EducationalInstitution);
   
            Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repository, subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} does not exist"); 

            SubscriberEducationHistory EducationHistory = await SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(_repository, educationalHistoryGuid);
            if (EducationHistory == null )
                throw new NotFoundException($"Educational History  {educationalHistoryGuid} does not exist");

            if (EducationHistory.SubscriberId != subscriber.SubscriberId)
                throw new UnauthorizedAccessException();

            // Find or create the institution 
            EducationalInstitution educationalInstitution =  await EducationalInstitutionFactory.GetOrAdd(_repository, EducationHistoryDto.EducationalInstitution);
            int educationalInstitutionId = educationalInstitution.EducationalInstitutionId;
            // Find or create the degree major 
            EducationalDegree educationalDegree = await EducationalDegreeFactory.GetOrAdd(_repository, EducationHistoryDto.EducationalDegree);
            int educationalDegreeId = educationalDegree.EducationalDegreeId;
            // Find or create the degree type 
            EducationalDegreeType educationalDegreeType =  await _repository.EducationalDegreeTypeRepository.GetByGuid(EducationHistoryDto.EducationalDegreeTypeGuid.Value);

            int educationalDegreeTypeId = 0;
            if (educationalDegreeType == null)
                educationalDegreeType = await EducationalDegreeTypeFactory.GetOrAdd(_repository, UpDiddyLib.Helpers.Constants.NotSpecifedOption);
            educationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId;

            EducationHistory.ModifyDate = DateTime.UtcNow;
            EducationHistory.StartDate = EducationHistoryDto.StartDate;
            EducationHistory.EndDate = EducationHistoryDto.EndDate;
            EducationHistory.DegreeDate = EducationHistoryDto.DegreeDate;
            EducationHistory.EducationalDegreeId = educationalDegreeId;
            EducationHistory.EducationalDegreeTypeId = educationalDegreeTypeId;
            EducationHistory.EducationalInstitutionId = educationalInstitutionId;
            _db.SaveChanges();

            // update google profile 
            _hangfireService.Enqueue<ScheduledJobs>(j => j.CloudTalentAddOrUpdateProfile(subscriber.SubscriberGuid.Value));
 
            return true;
        }



        public async Task<bool> DeleteEducationalHistory(Guid subscriberGuid, Guid educationalHistoryGuid)
        {
 
            Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repository, subscriberGuid);
            SubscriberEducationHistory EducationHistory = await SubscriberEducationHistoryFactory.GetEducationHistoryByGuid(_repository, educationalHistoryGuid);

            if (EducationHistory == null )
               throw new NotFoundException($"Educational History {educationalHistoryGuid} does not exist");

            if (EducationHistory.SubscriberId != subscriber.SubscriberId)
               throw new UnauthorizedAccessException();

            // Soft delete of the workhistory item
            EducationHistory.IsDeleted = 1;
            await _repository.SaveAsync();
 
            return true;
        }


        public async Task<List<SubscriberEducationHistoryDto>> GetEducationalHistory(Guid subscriberGuid)
        {
            
            Subscriber subscriber = await SubscriberFactory.GetSubscriberByGuid(_repository, subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException($"Subscriber {subscriberGuid} does not exist");

            var educationHistory = await _repository.SubscriberEducationHistoryRepository.GetAllWithTracking()
            .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriber.SubscriberId)
            .OrderByDescending(s => s.StartDate)
            .Select(eh => new SubscriberEducationHistory()
            {
                CreateDate = eh.CreateDate,
                CreateGuid = eh.CreateGuid,
                DegreeDate = eh.DegreeDate,
                EducationalDegree = new EducationalDegree()
                {
                    CreateDate = eh.EducationalDegree.CreateDate,
                    CreateGuid = eh.EducationalDegree.CreateGuid,
                    Degree = HttpUtility.HtmlDecode(eh.EducationalDegree.Degree),
                    EducationalDegreeGuid = eh.EducationalDegree.EducationalDegreeGuid,
                    EducationalDegreeId = eh.EducationalDegree.EducationalDegreeId,
                    IsDeleted = eh.EducationalDegree.IsDeleted,
                    ModifyDate = eh.EducationalDegree.ModifyDate,
                    ModifyGuid = eh.EducationalDegree.ModifyGuid
                },
                EducationalDegreeId = eh.EducationalDegreeId,
                EducationalDegreeType = eh.EducationalDegreeType,         
                EducationalDegreeTypeId = eh.EducationalDegreeTypeId,
                EducationalInstitution = new EducationalInstitution()
                {
                    CreateDate = eh.EducationalInstitution.CreateDate,
                    CreateGuid = eh.EducationalInstitution.CreateGuid,
                    EducationalInstitutionGuid = eh.EducationalInstitution.EducationalInstitutionGuid,
                    EducationalInstitutionId = eh.EducationalInstitution.EducationalInstitutionId,
                    IsDeleted = eh.EducationalInstitution.IsDeleted,
                    ModifyDate = eh.EducationalInstitution.ModifyDate,
                    ModifyGuid = eh.EducationalInstitution.ModifyGuid,
                    Name = HttpUtility.HtmlDecode(eh.EducationalInstitution.Name)
                },
                EducationalInstitutionId = eh.EducationalInstitutionId,
                EndDate = eh.EndDate,
                IsDeleted = eh.IsDeleted,
                ModifyDate = eh.ModifyDate,
                ModifyGuid = eh.ModifyGuid,
                StartDate = eh.StartDate,
                SubscriberEducationHistoryGuid = eh.SubscriberEducationHistoryGuid,
                SubscriberEducationHistoryId = eh.SubscriberEducationHistoryId,
                SubscriberId = eh.SubscriberId
                // ignoring Subscriber property
            })
            .ProjectTo<SubscriberEducationHistoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

            return educationHistory;
        }









    }
}
