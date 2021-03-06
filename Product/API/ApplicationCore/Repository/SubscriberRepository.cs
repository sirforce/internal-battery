﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;
using UpDiddyLib.Dto;
using SkillDto = UpDiddyLib.Domain.Models.SkillDto;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberRepository : UpDiddyRepositoryBase<Subscriber>, ISubscriberRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        private readonly ISubscriberGroupRepository _subscriberGroupRepository;
        private readonly IGroupPartnerRepository _groupPartnerRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IStoredProcedureRepository _storedProcedureRepository;

        public SubscriberRepository(UpDiddyDbContext dbContext, ISubscriberGroupRepository subscriberGroupRepository, IGroupPartnerRepository groupPartnerRepository, IPartnerRepository partnerRepository, IStoredProcedureRepository storedProcedureRepository) : base(dbContext)
        {
            _subscriberGroupRepository = subscriberGroupRepository;
            _groupPartnerRepository = groupPartnerRepository;
            _partnerRepository = partnerRepository;
            _storedProcedureRepository = storedProcedureRepository;
            _dbContext = dbContext;
        }

        public IQueryable<Subscriber> GetAllSubscribersAsync()
        {
            return GetAll();
        }

        public async Task<List<SubscriberTraining>> GetCandidateTrainingHistory(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var subscriberTrainingQuery = _dbContext.SubscriberTraining
                .Where(st => st.IsDeleted == 0 && st.Subscriber.IsDeleted == 0 && st.Subscriber.SubscriberGuid == subscriberGuid)
                .Include(st => st.Subscriber)
                .Include(st => st.TrainingType)
                .Skip(limit * offset)
                .Take(limit);

            //sorting            
            if (order.ToLower() == "descending")
            {
                switch (sort.ToLower())
                {
                    case "createdate":
                        subscriberTrainingQuery = subscriberTrainingQuery.OrderByDescending(s => s.CreateDate);
                        break;
                    case "modifydate":
                        subscriberTrainingQuery = subscriberTrainingQuery.OrderByDescending(s => s.ModifyDate);
                        break;
                    default:
                        subscriberTrainingQuery = subscriberTrainingQuery.OrderByDescending(s => s.ModifyDate);
                        break;
                }
            }
            else
            {
                switch (sort.ToLower())
                {
                    case "createdate":
                        subscriberTrainingQuery = subscriberTrainingQuery.OrderBy(s => s.CreateDate);
                        break;
                    case "modifydate":
                        subscriberTrainingQuery = subscriberTrainingQuery.OrderBy(s => s.ModifyDate);
                        break;
                    default:
                        subscriberTrainingQuery = subscriberTrainingQuery.OrderBy(s => s.ModifyDate);
                        break;
                }
            }

            return await subscriberTrainingQuery.ToListAsync();

        }

        public async Task<Subscriber> GetSubscriberByEmailAsync(string email)
        {
            var subscriberResult = await _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.Email == email)
                              .FirstOrDefaultAsync();

            return subscriberResult;
        }

        // get the parter attributed with source attribution (i.e. caused the subscriber to join cc )
        public async Task<SubscriberSourceDto> GetSubscriberSource(int subscriberId)
        {
            var sources = await _storedProcedureRepository.GetSubscriberSources(subscriberId);
            return sources
                .Where(s => s.PartnerRank == 1 && s.GroupRank == 1)
                .FirstOrDefault();
        }


        public Subscriber GetSubscriberByEmail(string email)
        {
            var subscriberResult = _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.Email == email)
                              .FirstOrDefault();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberByGuidAsync(Guid subscriberGuid)
        {
            var subscriberResult = await _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .FirstOrDefaultAsync();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberAccountDetailsByGuidAsync(Guid subscriberGuid)
        {
            var subscriberResult = await _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .FirstOrDefaultAsync();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberPersonalInfoByGuidAsync(Guid subscriberGuid)
        {
            var subscriberResult = await _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .Include(s => s.State)
                              .FirstOrDefaultAsync();

            return subscriberResult;
        }

        public async Task<Subscriber> GetSubscriberByIdAsync(int subscriberId)
        {
            return await _dbContext.Subscriber
              .Where(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId)
              .Include(s => s.State)
              .FirstOrDefaultAsync();
        }

        public async Task<Subscriber> GetCandidateEmploymentPreferencesBySubscriberGuidAsync(Guid subscriberGuid)
        {
            var subscriberEmploymentTypes = await _dbContext.SubscriberEmploymentTypes
                              .Where(s => s.Subscriber.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                              .Include(s => s.EmploymentType)
                              .Include(s => s.Subscriber.CommuteDistance)
                              .ToListAsync();

            if (subscriberEmploymentTypes == null || subscriberEmploymentTypes.Count == 0)
                return await _dbContext.Subscriber
                    .Where(s => s.SubscriberGuid == subscriberGuid && s.IsDeleted == 0)
                    .Include(s => s.CommuteDistance)
                    .FirstOrDefaultAsync();

            return subscriberEmploymentTypes.FirstOrDefault().Subscriber;
        }

        public Subscriber GetSubscriberByGuid(Guid subscriberGuid)
        {


            var subscriberResult = _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .FirstOrDefault();

            return subscriberResult;
        }

        public async Task<IList<Partner>> GetPartnersAssociatedWithSubscriber(int subscriberId)
        {


            var subscriberGroups = _subscriberGroupRepository.GetAll()
                .Where(s => s.SubscriberId == subscriberId);

            var groupPartners = _groupPartnerRepository.GetAll();
            var partners = _partnerRepository.GetAll();

            return await subscriberGroups
                .Join(groupPartners, sg => sg.GroupId, gp => gp.GroupId, (sg, gp) => new { sg, gp })
                .Join(partners, sg_gp => sg_gp.gp.PartnerId, partner => partner.PartnerId, (sg_gp, partner) => new Partner()
                {
                    ModifyDate = partner.ModifyDate,
                    LogoUrl = partner.LogoUrl,
                    IsDeleted = partner.IsDeleted,
                    ApiToken = partner.ApiToken,
                    CreateDate = partner.CreateDate,
                    CreateGuid = partner.CreateGuid,
                    Description = partner.Description,
                    ModifyGuid = partner.ModifyGuid,
                    Name = partner.Name,
                    PartnerGuid = partner.PartnerGuid,
                    PartnerId = partner.PartnerId,
                    PartnerType = partner.PartnerType,
                    PartnerTypeId = partner.PartnerTypeId,
                    Referrers = partner.Referrers,
                    WebRedirect = partner.WebRedirect
                })
                .ToListAsync<Partner>();

        }

        public async Task<int> GetSubscribersCountByStartEndDates(DateTime? startDate = null, DateTime? endDate = null)
        {
            //get queryable object for subscribers
            var queryableSubscribers = GetAll();

            if (startDate.HasValue)
            {
                queryableSubscribers = queryableSubscribers.Where(s => s.CreateDate >= startDate);
            }
            if (endDate.HasValue)
            {
                queryableSubscribers = queryableSubscribers.Where(s => s.CreateDate < endDate);
            }

            return await queryableSubscribers.Where(s => s.IsDeleted == 0).CountAsync();
        }

        public async Task UpdateHubSpotDetails(Guid subscriberId, long hubSpotVid)
        {
            var subscriber = await _dbContext.Subscriber
                .SingleOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberId);

            await UpdateHubSpotDetails(subscriber, hubSpotVid);
        }

        public async Task UpdateHubSpotDetails(int subscriberId, long hubSpotVid)
        {
            var subscriber = await _dbContext.Subscriber
                .SingleOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberId == subscriberId);

            await UpdateHubSpotDetails(subscriber, hubSpotVid);
        }

        public async Task<RolePreferenceDto> GetRolePreference(Guid subscriberGuid)
        {
            var subscriber = await _dbContext.Subscriber
                .Include(s => s.SubscriberSkills)
                    .ThenInclude(ss => ss.Skill)
                .Include(s => s.SubscriberLinks)
                .FirstOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid);

            if (subscriber == null) { return null; }

            var rolePreference = new RolePreferenceDto
            {
                JobTitle = subscriber.Title ?? string.Empty,
                DreamJob = subscriber.DreamJob ?? string.Empty,
                WhatSetsMeApart = subscriber.CurrentRoleProficiencies ?? string.Empty,
                WhatKindOfLeader = subscriber.PreferredLeaderStyle ?? string.Empty,
                WhatKindOfTeam = subscriber.PreferredTeamType ?? string.Empty,
                VolunteerOrPassionProjects = subscriber.PassionProjectsDescription ?? string.Empty,
                SkillGuids = subscriber.SubscriberSkills
                    .Where(ss => ss.IsDeleted == 0 && ss.Skill.SkillGuid.HasValue)
                    .Select(ss => ss.Skill.SkillGuid.Value)
                    .ToList(),
                SocialLinks = subscriber.SubscriberLinks
                    .Where(sl => sl.IsDeleted == 0)
                    .Select(sl => new SocialLinksDto
                    {
                        FriendlyName = sl.Label,
                        Url = sl.Url
                    })
                    .ToList(),
                ElevatorPitch = subscriber.CoverLetter ?? string.Empty
            };

            return rolePreference;
        }

        public async Task UpdateRolePreference(Guid subscriberGuid, RolePreferenceDto rolePreference)
        {
            if (rolePreference == null) { throw new ArgumentNullException(nameof(rolePreference)); }

            var subscriber = await _dbContext.Subscriber
                .Include(s => s.SubscriberSkills)
                    .ThenInclude(ss => ss.Skill)
                .Include(s => s.SubscriberLinks)
                .FirstOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid);

            if (subscriber == null) { return; }

            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                subscriber.Title = rolePreference.JobTitle;
                subscriber.CurrentRoleProficiencies = rolePreference.DreamJob;
                subscriber.DreamJob = rolePreference.WhatSetsMeApart;
                subscriber.PreferredLeaderStyle = rolePreference.WhatKindOfLeader;
                subscriber.PreferredTeamType = rolePreference.WhatKindOfTeam;
                subscriber.PassionProjectsDescription = rolePreference.VolunteerOrPassionProjects;
                subscriber.CoverLetter = rolePreference.ElevatorPitch;

                foreach (var linkToUpdate in subscriber.SubscriberLinks)
                {
                    var link = rolePreference.SocialLinks
                        .FirstOrDefault(l => l.FriendlyName == linkToUpdate.Label);
                    if (link != null) { linkToUpdate.Url = link.Url; }
                }

                var linksToDelete = subscriber.SubscriberLinks
                    .Where(link => link.IsDeleted == 0 && !rolePreference.SocialLinks.Any(sl => sl.FriendlyName == link.Label));

                foreach (var linkToDelete in linksToDelete) { linkToDelete.IsDeleted = 1; }

                var linksToAdd = rolePreference.SocialLinks
                    .Where(link => !subscriber.SubscriberLinks.Any(sl => sl.IsDeleted == 0 && sl.Label == link.FriendlyName))
                    .Select(link => new SubscriberLink
                    {
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.NewGuid(),
                        SubscriberLinkGuid = Guid.NewGuid(),
                        SubscriberId = subscriber.SubscriberId,
                        IsDeleted = 0,
                        Label = link.FriendlyName,
                        Url = link.Url
                    })
                    .ToList();

                if (linksToAdd.Any())
                {
                    await _dbContext.SubscriberLinks.AddRangeAsync(linksToAdd);
                }

                await _dbContext.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            finally { transaction.Dispose(); }
        }

        private async Task UpdateHubSpotDetails(Subscriber subscriber, long hubSpotVid)
        {
            subscriber.HubSpotVid = hubSpotVid;
            subscriber.HubSpotModifyDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateCandidateEmploymentPreferencesBySubscriberGuidAsync(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto)
        {
            //get all SubscriberEmploymentTypes, include deleted ones.
            var subscriberEmploymentTypes = _dbContext.SubscriberEmploymentTypes
                              .Where(s => s.Subscriber.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid)
                              .Include(s => s.Subscriber)
                              .Include(s => s.EmploymentType)
                              .Include(s => s.Subscriber.CommuteDistance)
                              .ToList();
            var subscriber = subscriberEmploymentTypes.FirstOrDefault()?.Subscriber ??
                                await _dbContext.Subscriber
                                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                                              .FirstOrDefaultAsync();

            //get all requested EmploymentTypes
            var employmentTypes = _dbContext.EmploymentType
                .Where(et => candidateEmploymentPreferenceDto.EmploymentTypeGuids != null &&
                            candidateEmploymentPreferenceDto.EmploymentTypeGuids.Count > 0 &&
                            candidateEmploymentPreferenceDto.EmploymentTypeGuids.Contains(et.EmploymentTypeGuid) &&
                            et.IsDeleted == 0)
                .ToList();

            //ignore deleted ones when filtering to logically delete subscriberEmploymentTypes
            var subscriberEmploymentTypesToDelete = subscriberEmploymentTypes
                .Where(del => del.IsDeleted == 0 &&
                            !employmentTypes.Select(et => et.EmploymentTypeGuid).ToList().Contains(del.EmploymentType.EmploymentTypeGuid))
                .Select(set => set.EmploymentType.EmploymentTypeGuid)
                .ToList();

            //check against all the subscriberEmploymentTypes, including deleted ones.
            var subscriberEmploymentTypesToAdd = employmentTypes.Select(et => et.EmploymentTypeGuid).ToList()
                .Where(add =>
                        !subscriberEmploymentTypes
                        .Select(set => set.EmploymentType.EmploymentTypeGuid).ToList().Contains(add))
                .ToList();

            //Updates will happen when we logically undelete a SubscriberEmploymentTypes
            //Check againsted the deleted subscriberEmploymentTypes
            var subscriberEmploymentTypesToUpdate = employmentTypes.Select(et => et.EmploymentTypeGuid).ToList()
                .Where(add =>
                         subscriberEmploymentTypes
                        .Where(set => set.IsDeleted == 1)
                        .Select(set => set.EmploymentType.EmploymentTypeGuid).ToList().Contains(add))
                .ToList();

            //logically delete SubscriberEmploymentTypes
            foreach (var employmentTypesToDelete in subscriberEmploymentTypesToDelete)
            {
                var recordToDelete = subscriberEmploymentTypes.FirstOrDefault(set => set.EmploymentType.EmploymentTypeGuid == employmentTypesToDelete);
                recordToDelete.IsDeleted = 1;
                recordToDelete.ModifyDate = DateTime.UtcNow;
            }

            //logically undelete SubscriberEmploymentTypes
            foreach (var employmentTypesToUpdate in subscriberEmploymentTypesToUpdate)
            {
                var recordToUpdate = subscriberEmploymentTypes.FirstOrDefault(set => set.EmploymentType.EmploymentTypeGuid == employmentTypesToUpdate);
                recordToUpdate.IsDeleted = 0;
                recordToUpdate.ModifyDate = DateTime.UtcNow;
            }

            //Add new SubscriberEmploymentTypes
            foreach (var employmentTypesToAdd in subscriberEmploymentTypesToAdd)
            {
                _dbContext.SubscriberEmploymentTypes.Add(new SubscriberEmploymentTypes
                {
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    IsDeleted = 0,
                    SubscriberEmploymentTypesGuid = Guid.NewGuid(),
                    SubscriberId = subscriber.SubscriberId,
                    EmploymentType = employmentTypes.FirstOrDefault(et => et.EmploymentTypeGuid == employmentTypesToAdd)
                });
            }

            var commuteDistance = _dbContext.CommuteDistance
                .FirstOrDefault(cd => candidateEmploymentPreferenceDto.CommuteDistanceGuid.HasValue &&
                                      cd.CommuteDistanceGuid == candidateEmploymentPreferenceDto.CommuteDistanceGuid);

            subscriber.IsFlexibleWorkScheduleRequired = candidateEmploymentPreferenceDto.IsFlexibleWorkScheduleRequired;
            subscriber.IsWillingToTravel = candidateEmploymentPreferenceDto.IsWillingToTravel;
            subscriber.CommuteDistanceId = commuteDistance?.CommuteDistanceId;

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSubscriberPersonalInfo(Guid subscriberGuid, State subscriberState, CandidatePersonalInfoDto candidatePersonalInfoDto)
        {
            var subscriber = await _dbContext.Subscriber
                              .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                              .FirstOrDefaultAsync();

            subscriber.FirstName = candidatePersonalInfoDto?.FirstName;
            subscriber.LastName = candidatePersonalInfoDto?.LastName;
            subscriber.PhoneNumber = candidatePersonalInfoDto?.MobilePhone;
            subscriber.Address = candidatePersonalInfoDto?.StreetAddress;
            subscriber.City = candidatePersonalInfoDto?.City;
            subscriber.StateId = subscriberState?.StateId;
            subscriber.PostalCode = candidatePersonalInfoDto?.Postal;
            subscriber.ModifyDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Language>> GetLanguages()
            => await _dbContext.Languages
                .Where(l => l.IsDeleted == 0)
                .OrderBy(l => l.LanguageName)
                .ToListAsync();

        public async Task<List<ProficiencyLevel>> GetProficiencyLevels()
            => await _dbContext.ProficiencyLevels
                .Where(pl => pl.IsDeleted == 0)
                .OrderBy(pl => pl.Sequence)
                .ToListAsync();

        public async Task<List<SubscriberLanguageProficiency>> GetSubscriberLanguageProficiencies(Guid subscriberGuid)
            => await _dbContext.SubscriberLanguageProficiencies
                .Include(slp => slp.Subscriber)
                .Include(slp => slp.Language)
                .Include(slp => slp.ProficiencyLevel)
                .Where(slp =>
                    slp.Subscriber.IsDeleted == 0 &&
                    slp.Subscriber.SubscriberGuid == subscriberGuid &&
                    slp.IsDeleted == 0
                )
                .ToListAsync();

        public async Task<Guid> CreateSubscriberLanguageProficiency(LanguageProficiencyDto languageProficiencyDto, Guid subscriberGuid)
        {
            if (languageProficiencyDto == null) { throw new ArgumentNullException(nameof(languageProficiencyDto)); }

            var existingLanguageProficiency = await _dbContext.SubscriberLanguageProficiencies
                .Include(slp => slp.Subscriber)
                .Include(slp => slp.Language)
                .Where(slp =>
                    slp.Subscriber.SubscriberGuid == subscriberGuid &&
                    slp.Language.LanguageGuid == languageProficiencyDto.LanguageGuid)
                .SingleOrDefaultAsync();

            if (existingLanguageProficiency?.IsDeleted == 0)
            {
                throw new AlreadyExistsException("This language entry already exists");
            }
            else if (existingLanguageProficiency?.IsDeleted == 1)
            {
                var proficiencyLevel = await _dbContext.ProficiencyLevels
                    .SingleOrDefaultAsync(pl => pl.IsDeleted == 0 && pl.ProficiencyLevelGuid == languageProficiencyDto.ProficiencyLevelGuid);
                if (proficiencyLevel == null) { throw new NotFoundException("Couldn't find the ProficiencyLevel"); }

                existingLanguageProficiency.IsDeleted = 0;
                existingLanguageProficiency.ProficiencyLevel = proficiencyLevel;
                existingLanguageProficiency.ModifyDate = DateTime.UtcNow;
                existingLanguageProficiency.ModifyGuid = Guid.NewGuid();
            }
            else
            {
                var subscriber = await _dbContext.Subscriber
                    .SingleOrDefaultAsync(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid);

                var language = await _dbContext.Languages
                    .SingleOrDefaultAsync(l => l.IsDeleted == 0 && l.LanguageGuid == languageProficiencyDto.LanguageGuid);
                if (language == null) { throw new NotFoundException("Couldn't find the language"); }

                var proficiencyLevel = await _dbContext.ProficiencyLevels
                    .SingleOrDefaultAsync(pl => pl.IsDeleted == 0 && pl.ProficiencyLevelGuid == languageProficiencyDto.ProficiencyLevelGuid);
                if (proficiencyLevel == null) { throw new NotFoundException("Couldn't find the proficiency level"); }

                existingLanguageProficiency = new SubscriberLanguageProficiency
                {
                    IsDeleted = 0,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.NewGuid(),
                    SubscriberLanguageProficiencyGuid = Guid.NewGuid(),
                    Subscriber = subscriber,
                    Language = language,
                    ProficiencyLevel = proficiencyLevel
                };

                await _dbContext.SubscriberLanguageProficiencies
                    .AddAsync(existingLanguageProficiency);
            }

            await _dbContext.SaveChangesAsync();
            return existingLanguageProficiency.SubscriberLanguageProficiencyGuid;
        }

        public async Task UpdateSubscriberLanguageProficiency(LanguageProficiencyDto languageProficiencyDto, Guid subscriberGuid)
        {
            if (languageProficiencyDto == null) { throw new ArgumentNullException(nameof(languageProficiencyDto)); }

            var languageProficiencies = await GetSubscriberLanguageProficiencies(subscriberGuid);
            var existingLanguageProficiency = languageProficiencies
                .SingleOrDefault(slp => slp.SubscriberLanguageProficiencyGuid == languageProficiencyDto.LanguageProficiencyGuid);

            if (existingLanguageProficiency == null) { throw new NotFoundException("Couldn't find an existing language proficiency entry"); }

            if (languageProficiencyDto.LanguageGuid != existingLanguageProficiency.Language.LanguageGuid)
            {
                if (languageProficiencies.Any(slp => slp.Language.LanguageGuid == languageProficiencyDto.LanguageGuid))
                {
                    throw new AlreadyExistsException("Please select a different language");
                }

                var language = await _dbContext.Languages
                    .SingleOrDefaultAsync(l => l.IsDeleted == 0 && l.LanguageGuid == languageProficiencyDto.LanguageGuid);
                if (language == null) { throw new NotFoundException("Couldn't find the language"); }
                existingLanguageProficiency.Language = language;
            }

            if (languageProficiencyDto.ProficiencyLevelGuid != existingLanguageProficiency.ProficiencyLevel.ProficiencyLevelGuid)
            {
                var proficiencyLevel = await _dbContext.ProficiencyLevels
                    .SingleOrDefaultAsync(pl => pl.IsDeleted == 0 && pl.ProficiencyLevelGuid == languageProficiencyDto.ProficiencyLevelGuid);
                if (proficiencyLevel == null) { throw new NotFoundException("Couldn't find the proficiency level"); }

                existingLanguageProficiency.ProficiencyLevel = proficiencyLevel;
            }
            existingLanguageProficiency.ModifyDate = DateTime.UtcNow;
            existingLanguageProficiency.ModifyGuid = Guid.NewGuid();

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteSubscriberLanguageProficiency(Guid languageProficiencyGuid, Guid subscriberGuid)
        {
            var existingLanguageProficiency = await _dbContext.SubscriberLanguageProficiencies
                .Include(slp => slp.Subscriber)
                .SingleOrDefaultAsync(slp =>
                    slp.IsDeleted == 0 &&
                    slp.Subscriber.SubscriberGuid == subscriberGuid &&
                    slp.SubscriberLanguageProficiencyGuid == languageProficiencyGuid);
            if (existingLanguageProficiency == null) { throw new NotFoundException("Couldn't find an existing language proficiency entry"); }

            existingLanguageProficiency.IsDeleted = 1;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Guid?> UpdateEmailVerificationStatus(string email, bool isVerified)
        {
            Guid? subscriberGuid = null;

            var subscriber = await _dbContext.Subscriber
                .Where(s => s.IsDeleted == 0 && s.Email == email)
                .FirstOrDefaultAsync();

            if (subscriber != null)
            {
                subscriberGuid = subscriber.SubscriberGuid;
                subscriber.IsVerified = isVerified;
                subscriber.ModifyDate = DateTime.UtcNow;
                subscriber.ModifyGuid = Guid.Empty;
                await _dbContext.SaveChangesAsync();
            }

            return subscriberGuid;
        }

        public async Task<List<WorkHistoryDto>> GetCandidateWorkHistory(Guid subscriberGuid, int limit, int offset, string sort, string order)
        {
            var spParams = new object[] {
                new SqlParameter("@SubscriberGuid", subscriberGuid),
                new SqlParameter("@Limit", limit),
                new SqlParameter("@Offset", offset),
                new SqlParameter("@Sort", sort),
                new SqlParameter("@Order", order),
                };
            List<WorkHistoryDto> workHistories = null;
            workHistories = await _dbContext.WorkHistories.FromSql<WorkHistoryDto>("[dbo].[System_Get_SubscriberWorkHistory] @SubscriberGuid, @Limit, @Offset, @Sort, @Order", spParams).ToListAsync();
            return workHistories;
        }

        public async Task UpdateCandidateWorkHistory(Guid subscriber, WorkHistoryUpdateDto request)
        {
            var subscriberGuid = new SqlParameter("@SubscriberGuid", subscriber);
            DataTable subscriberWorkHistoryTable = new DataTable();
            subscriberWorkHistoryTable.Columns.Add("StartDate", typeof(DateTime));
            subscriberWorkHistoryTable.Columns.Add("CompanyName", typeof(string));
            subscriberWorkHistoryTable.Columns.Add("EndDate", typeof(DateTime));
            subscriberWorkHistoryTable.Columns.Add("IsCurrent", typeof(bool));
            subscriberWorkHistoryTable.Columns.Add("JobDescription", typeof(string));
            subscriberWorkHistoryTable.Columns.Add("JobTitle", typeof(string));
            subscriberWorkHistoryTable.Columns.Add("SubscriberWorkHistoryGuid", typeof(Guid));

            if (request != null && request.WorkHistories.Count > 0)
            {
                foreach (var workHistory in request.WorkHistories)
                {
                    subscriberWorkHistoryTable.Rows.Add(workHistory.BeginDate, workHistory.CompanyName, workHistory.EndDate, workHistory.IsCurrent, workHistory.JobDescription, workHistory.JobTitle, workHistory.WorkHistoryGuid);
                }
            }
            var subscriberWorkHistory = new SqlParameter("@SubscriberWorkHistory", subscriberWorkHistoryTable);
            subscriberWorkHistory.SqlDbType = SqlDbType.Structured;
            subscriberWorkHistory.TypeName = "dbo.SubscriberWorkHistory";

            var spParams = new object[] { subscriberGuid, subscriberWorkHistory };
            var rowsAffected = _dbContext.Database.ExecuteSqlCommand(@"EXEC [dbo].[System_Update_SubscriberWorkHistory] @SubscriberGuid, @SubscriberWorkHistory", spParams);
        }
    }
}