﻿using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;
using AutoMapper;
using UpDiddyApi.Models;
using System.Collections.Generic;

namespace UpDiddyApi.ApplicationCore.Services.Candidate
{
    public class CandidatesService : ICandidatesService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISubscriberService _subscriberService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public CandidatesService(
            ILogger<CandidatesService> logger,
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ISubscriberService subscriberService
        )
        {
            _logger = logger;
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _subscriberService = subscriberService;
        }

        #region Personal Info
        public async Task<CandidatePersonalInfoDto> GetCandidatePersonalInfo(Guid subscriberGuid)
        {
            _logger.LogInformation($"CandidatesService:GetCandidatePersonalInfo begin.");

            if (subscriberGuid == Guid.Empty) throw new FailedValidationException($"CandidatesService:GetCandidatePersonalInfo subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
            try
            {
                var subscriber = await _repositoryWrapper.SubscriberRepository.GetSubscriberPersonalInfoByGuidAsync(subscriberGuid);
                return _mapper.Map<CandidatePersonalInfoDto>(subscriber);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetCandidatePersonalInfo  Error: {ex.ToString()} ");
                throw ex;
            }

        }

        public async Task UpdateCandidatePersonalInfo(Guid subscriberGuid, CandidatePersonalInfoDto candidatePersonalInfoDto)
        {
            _logger.LogInformation($"CandidatesService:UpdateCandidatePersonalInfo begin.");

            if (subscriberGuid == Guid.Empty) throw new FailedValidationException($"CandidatesService:UpdateCandidatePersonalInfo subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
            if (candidatePersonalInfoDto == null)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference candidatePersonalInfoDto cannot be null");

            try
            {
                Models.State candidateState = null;
                if (!String.IsNullOrWhiteSpace(candidatePersonalInfoDto.State) && candidatePersonalInfoDto.State.Trim().Length == 2)
                {
                    candidateState = await _repositoryWrapper.State.GetUSCanadaStateByCode(candidatePersonalInfoDto.State.Trim());
                    //add the missing address and then return
                    if (candidateState == null)
                    {
                        //add state if not recognised - assume its USA.
                        var country = await _repositoryWrapper.Country.GetCountryByCode3("USA");
                        await _repositoryWrapper.State.AddUSState(new State
                        {
                            CreateDate = DateTime.UtcNow,
                            CreateGuid = Guid.Empty,
                            StateGuid = Guid.NewGuid(),
                            Code = candidatePersonalInfoDto?.State,
                            CountryId = country.CountryId,
                            Name = candidatePersonalInfoDto?.State //Name will the new state code.
                                                                   //Sequence will default to 0
                        });

                        candidateState = await _repositoryWrapper.State.GetUSCanadaStateByCode(candidatePersonalInfoDto.State.Trim());
                        if (candidateState == null)
                            throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference newly added state:{candidatePersonalInfoDto.State}, not found.");
                    }
                }
                await _repositoryWrapper.SubscriberRepository.UpdateSubscriberPersonalInfo(subscriberGuid, candidateState, candidatePersonalInfoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:UpdateCandidatePersonalInfo  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:UpdateCandidatePersonalInfo begin.");

        }
        #endregion Personal Info

        #region Employment Preferences

        public async Task<CandidateEmploymentPreferenceDto> GetCandidateEmploymentPreference(Guid subscriberGuid)
        {
            _logger.LogInformation($"CandidatesService:GetCandidateEmploymentPreference begin.");

            if (subscriberGuid == Guid.Empty) throw new FailedValidationException($"CandidatesService:GetCandidateEmploymentPreference subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
            try
            {
                var subscriberEmploymentTypes = await _repositoryWrapper.SubscriberRepository.GetCandidateEmploymentPreferencesBySubscriberGuidAsync(subscriberGuid);

                if (subscriberEmploymentTypes == null)
                {
                    throw new FailedValidationException($"CandidatesService:GetCandidateEmploymentPreference Cannot locate subscriber: {subscriberGuid}");
                }

                return _mapper.Map<CandidateEmploymentPreferenceDto>(subscriberEmploymentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetCandidateEmploymentPreference  Error: {ex.ToString()} ");
                throw ex;
            }

        }

        public async Task UpdateCandidateEmploymentPreference(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto)
        {
            _logger.LogInformation($"CandidatesService:UpdateCandidateEmploymentPreference begin.");

            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference subscriber guid cannot be empty({subscriberGuid})");
            if (candidateEmploymentPreferenceDto == null)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEmploymentPreference candidateEmploymentPreferenceDto cannot be null");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");
            try
            {
                await _repositoryWrapper.SubscriberRepository.UpdateCandidateEmploymentPreferencesBySubscriberGuidAsync(subscriberGuid, candidateEmploymentPreferenceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:UpdateCandidateEmploymentPreference  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"CandidatesService:UpdateCandidateEmploymentPreference end.");
        }
        #endregion Employment Preferences

        #region Role Preferences

        public async Task<RolePreferenceDto> GetRolePreference(Guid subscriberGuid)
        {
            try
            {
                _logger.LogDebug("CandidatesService:GetRolePreference: Fetching Candidate 360 Role information for {subscriber}", subscriberGuid);

                var rolePreference = await _repositoryWrapper.SubscriberRepository.GetRolePreference(subscriberGuid);
                _logger.LogDebug("CandidatesService:GetRolePreference: Returning Candidate 360 Role information for {subscriber}", subscriberGuid);

                return rolePreference;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:GetRolePreference: Error while fetching Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        public async Task UpdateRolePreference(Guid subscriberGuid, RolePreferenceDto rolePreference)
        {
            if (rolePreference == null) { throw new ArgumentNullException(nameof(rolePreference)); }

            var hasDuplicates = rolePreference.SocialLinks
                .GroupBy(sl => sl.FriendlyName)
                .Any(sl => sl.Count() > 1);

            if (hasDuplicates) { throw new FailedValidationException("Cannot specify more than one social link of the same type"); }

            try
            {
                _logger.LogDebug("CandidatesService:UpdateRolePreference: Updating Candidate 360 Role information for {subscriber}", subscriberGuid);

                await _repositoryWrapper.SubscriberRepository.UpdateRolePreference(subscriberGuid, rolePreference);
                _logger.LogDebug("CandidatesService:UpdateRolePreference: Updated Candidate 360 Role information for {subscriber}", subscriberGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CandidatesService:UpdateRolePreference: Error while updating Candidate 360 Role information for {subscriber}", subscriberGuid);
                throw;
            }
        }

        #endregion Role Preferences

        #region Skills

        public async Task<SkillListDto> GetSkills(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NotFoundException("subscriberGuid cannot be null or empty");

            var candidateSkills = await _repositoryWrapper.SubscriberSkillRepository.GetCandidateSkills(subscriberGuid, limit, offset, sort, order);
           
            return _mapper.Map<SkillListDto>(candidateSkills);
        }
        public async Task UpdateSkills(Guid subscriberGuid, List<string> skillNames)
        {
            if (subscriberGuid == null || subscriberGuid == Guid.Empty)
                throw new NotFoundException("subscriberGuid cannot be null or empty");

            await _repositoryWrapper.SubscriberSkillRepository.UpdateCandidateSkills(subscriberGuid, skillNames);
        }

        #endregion

        #region Education & Training
        public async Task<EducationalDegreeTypeListDto> GetAllEducationalDegrees(int limit, int offset, string sort, string order)
        {
            _logger.LogInformation($"CandidatesService:GetAllEducationalDegrees begin.");

            try
            {
                var educationalDegrees = await _repositoryWrapper.EducationalDegreeTypeRepository.GetAllDefinedEducationDegreeTypes(limit, offset, sort, order);
                if (educationalDegrees == null) return null;
                return _mapper.Map<EducationalDegreeTypeListDto>(educationalDegrees);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetAllEducationalDegrees  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:GetAllEducationalDegrees end.");

        }

        public async Task<SubscriberEducationHistoryDto> GetCandidateEducationHistory(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            _logger.LogInformation($"CandidatesService:GetCandidateEducationHistory begin.");
            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"CandidatesService:GetCandidateEducationHistory subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

            try
            {
                var candidateEducationHistory = await _repositoryWrapper.SubscriberEducationHistoryRepository.GetEducationalHistoryBySubscriberGuid(subscriberGuid, limit, offset, sort, order);
                return _mapper.Map<SubscriberEducationHistoryDto>(candidateEducationHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetCandidateEducationHistory  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:GetCandidateEducationHistory end.");
        }

        public async Task<TrainingTypesDto> GetAllTrainingTypes(int limit, int offset, string sort, string order)
        {
            _logger.LogInformation($"CandidatesService:GetAllTrainingTypes begin.");

            try
            {
                var trainingTypes = await _repositoryWrapper.TrainingTypesRepository.GetAllTrainingTypes(limit, offset, sort, order);
                return _mapper.Map<TrainingTypesDto>(trainingTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetAllTrainingTypes  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:GetAllTrainingTypes end.");
        }

        public async Task<SubscriberTrainingHistoryDto> GetCandidateTrainingHistory(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            _logger.LogInformation($"CandidatesService:GetCandidateTrainingHistory begin.");
            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"CandidatesService:GetCandidateTrainingHistory subscriber guid cannot be empty({subscriberGuid})");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

            try
            {
                var candidateTrainingHistory = await _repositoryWrapper.SubscriberRepository.GetCandidateTrainingHistory(subscriberGuid, limit, offset, sort, order);
                if (candidateTrainingHistory == null) return null;
                return _mapper.Map<SubscriberTrainingHistoryDto>(candidateTrainingHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:GetCandidateTrainingHistory  Error: {ex.ToString()} ");
                throw ex;
            }
            _logger.LogInformation($"CandidatesService:GetCandidateTrainingHistory end.");
        }
        public async Task UpdateCandidateEducationAndTraining(Guid subscriberGuid, SubscriberEducationAssessmentsDto subscriberEducationAssessmentsDto)
        {
            _logger.LogInformation($"CandidatesService:UpdateCandidateEducationAndTraining begin.");
            if (subscriberEducationAssessmentsDto == null)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEducationAndTraining subscriberEducationAssessmentsDto cannot be null (subscriberGuid:{subscriberGuid})");

            if (subscriberGuid == Guid.Empty)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEducationAndTraining subscriber guid cannot be empty({subscriberGuid})");
            if (subscriberEducationAssessmentsDto == null)
                throw new FailedValidationException($"CandidatesService:UpdateCandidateEducationAndTraining candidateEmploymentPreferenceDto cannot be null");
            var Subscriber = await _subscriberService.GetSubscriberByGuid(subscriberGuid);
            if (Subscriber == null)
                throw new NotFoundException($"SubscriberGuid {subscriberGuid} does not exist exist");

            try
            {
                await _repositoryWrapper.SubscriberEducationHistoryRepository.UpdateCandidateEducationAndTraining(Subscriber.SubscriberId, subscriberEducationAssessmentsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CandidatesService:UpdateCandidateEducationAndTraining  Error: {ex.ToString()} ");
                throw ex;
            }

            _logger.LogInformation($"CandidatesService:UpdateCandidateEducationAndTraining end.");
        }

        #endregion
    }
}
