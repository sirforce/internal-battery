﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using Google.Protobuf.WellKnownTypes;
using Google.Apis.CloudTalentSolution.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using UpDiddyApi.ApplicationCore.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Hangfire;
using UpDiddyApi.Workflow;
using System.Net;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class JobPostingFactory
    {

        public static void SetMetaData(JobPosting jobPosting, JobPostingDto jobPostingDto)
        {
            // add meta data for seo 
            jobPostingDto.MetaDescription = $"Search for {jobPostingDto.Title} jobs near {jobPostingDto.CityProvince} with CareerCircle and find your next great opportunity today.";
            jobPostingDto.MetaTitle = jobPosting.Title;
            int numTerms = 3;
            jobPostingDto.MetaKeywords = jobPostingDto.CityProvince.Replace(',', ' ') + " Employment, Work, " + jobPostingDto.Title + " Jobs ";
            if (jobPostingDto.Industry != null)
            {
                jobPostingDto.MetaKeywords += ", " + jobPostingDto.Industry.Name + " Jobs ";

            }

            if (jobPostingDto.JobCategory != null)
            {
                jobPostingDto.MetaKeywords += ", " + jobPostingDto.JobCategory.Name + " Jobs ";
                ++numTerms;
            }

            // per foley - limit to 10 terms at max
            foreach (SkillDto s in jobPostingDto.JobPostingSkills)
            {
                jobPostingDto.MetaKeywords += ", " + s.SkillName + " Jobs";
                ++numTerms;
                // magic number ok since this is an industry standard that's not going to change 
                if (numTerms == 10)
                    break;
            }
        }

        public static string JobPostingFullyQualifiedUrl(IConfiguration config, JobPostingDto jobPostingDto)
        {


            string jobPostingUrl = config["Environment:BaseUrl"].TrimEnd('/') + Utils.CreateSemanticJobPath(
                 jobPostingDto.Industry == null ? string.Empty : jobPostingDto.Industry.Name,
                 jobPostingDto.JobCategory == null ? string.Empty : jobPostingDto.JobCategory.Name,
                 jobPostingDto.Country,
                 jobPostingDto.Province,
                 jobPostingDto.City,
                 jobPostingDto.JobPostingGuid.ToString()
                );
            return jobPostingUrl;
        }

        public static List<JobPosting> GetAllJobPostingsForSitemap(UpDiddyDbContext db)
        {
            // note that this doesn't include all related entities; only those that we need to build the semantic url
            return db.JobPosting
                .Include(jp => jp.Industry)
                .Include(jp => jp.JobCategory)
                .Where(s => s.IsDeleted == 0)
                .Select(jp => new JobPosting()
                {
                    JobPostingGuid = jp.JobPostingGuid,
                    Industry = new Industry() { Name = jp.Industry.Name },
                    JobCategory = new JobCategory() { Name = jp.JobCategory.Name },
                    Country = jp.Country,
                    Province = jp.Province,
                    City = jp.City,
                    ModifyDate = jp.ModifyDate.HasValue ? jp.ModifyDate.Value : jp.CreateDate
                })
                .ToList();
        }

        public static bool DeleteJob(UpDiddyDbContext db, Guid jobPostingGuid, ref string ErrorMsg, ILogger syslog, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {

            JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(db, jobPostingGuid);
            if (jobPosting == null)
            {
                ErrorMsg = $"Job posting {jobPostingGuid} does not exist";
                return false;
            }
            Recruiter recruiter = RecruiterFactory.GetRecruiterById(db, jobPosting.RecruiterId.Value);
            if (recruiter == null)
            {
                ErrorMsg = $"Recruiter {jobPosting.RecruiterId.Value} rec not found";
                return false;
            }

            if (jobPosting.RecruiterId != recruiter.RecruiterId)
            {
                ErrorMsg = "JobPosting owner is not specified or does not match user posting job";
                return false;
            }

            // queue a job to delete the posting from the job index and mark it as deleted in sql server
            BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentDeleteJob(jobPosting.JobPostingGuid));
            syslog.Log(LogLevel.Information, $"***** JobController:DeleteJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");
            return true;
        }

        public static bool PostJob(UpDiddyDbContext db, int recruiterId, JobPostingDto jobPostingDto, ref Guid newPostingGuid, ref string ErrorMsg, ILogger syslog, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration, bool isAcceptsNewSkills)
        {
            if (isAcceptsNewSkills)
            {
                var updatedSkills = new List<SkillDto>();
                foreach (var skillDto in jobPostingDto.JobPostingSkills)
                {
                    var skill = SkillFactory.GetOrAdd(db, skillDto.SkillName);
                    updatedSkills.Add(new SkillDto()
                    {
                        SkillGuid = skill.SkillGuid,
                        SkillName = skill.SkillName
                    });
                }
                jobPostingDto.JobPostingSkills = updatedSkills;
            }

            return PostJob(db, recruiterId, jobPostingDto, ref newPostingGuid, ref ErrorMsg, syslog, mapper, configuration);
        }

        public static bool PostJob(UpDiddyDbContext db, int recruiterId, JobPostingDto jobPostingDto, ref Guid newPostingGuid, ref string ErrorMsg, ILogger syslog, IMapper mapper, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            int postingTTL = int.Parse(configuration["JobPosting:PostingTTLInDays"]);

            if (jobPostingDto == null)
            {
                ErrorMsg = "JobPosting is required";
                return false;
            }

            syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting started at: {DateTime.UtcNow.ToLongDateString()}");

            JobPosting jobPosting = mapper.Map<JobPosting>(jobPostingDto);
            // todo find a better way to deal with the job posting having a collection of JobPostingSkill and the job posting DTO having a collection of SkillDto
            // ignore posting skills that were mapped via automapper, they will be associated with the posting below 
            jobPosting.JobPostingSkills = null;
            // assign recruiter
            jobPosting.RecruiterId = recruiterId;
            // use factory method to make sure all the base data values are set just 
            // in case the caller didn't set them
            BaseModelFactory.SetDefaultsForAddNew(jobPosting);
            // important! Init all reference object ids to null since further logic will use < 0 to check for 
            // their validity
            JobPostingFactory.SetDefaultsForAddNew(jobPosting);
            // Asscociate related objects that were passed by guid
            // todo find a more efficient way to do this
            JobPostingFactory.MapRelatedObjects(db, jobPosting, jobPostingDto);

            string msg = string.Empty;

            if (JobPostingFactory.ValidateJobPosting(jobPosting, configuration, ref msg) == false)
            {
                ErrorMsg = msg;
                syslog.Log(LogLevel.Warning, "JobPostingController.CreateJobPosting:: Bad Request {Description} {JobPosting}", msg, jobPostingDto);
                return false;
            }

            jobPosting.CloudTalentIndexStatus = (int)GoogleCloudIndexStatus.NotIndexed;
            jobPosting.JobPostingGuid = Guid.NewGuid();
            // set expiration date 
            if (jobPosting.PostingDateUTC < DateTime.UtcNow)
                jobPosting.PostingDateUTC = DateTime.UtcNow;
            if (jobPosting.PostingExpirationDateUTC < DateTime.UtcNow)
            {
                jobPosting.PostingExpirationDateUTC = DateTime.UtcNow.AddDays(postingTTL);
            }
            // save the job to sql server 
            // todo make saving the job posting and skills more efficient with a stored procedure 
            db.JobPosting.Add(jobPosting);
            db.SaveChanges();
            // save associated job posting skills 
            JobPostingFactory.SavePostingSkills(db, jobPosting, jobPostingDto);
            //index active jobs into google 
            if (jobPosting.JobStatus == (int)JobPostingStatus.Active)
                BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentAddJob(jobPosting.JobPostingGuid));


            newPostingGuid = jobPosting.JobPostingGuid;
            syslog.Log(LogLevel.Information, $"***** JobController:CreateJobPosting completed at: {DateTime.UtcNow.ToLongDateString()}");

            return true;

        }



        public static JobPosting GetJobPostingById(UpDiddyDbContext db, int jobPostingId)
        {
            return db.JobPosting
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Recruiter.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPostingId)
                .FirstOrDefault();
        }




        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static List<JobPosting> GetJobPostingsForSubscriber(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPosting
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Recruiter.Subscriber)
                .Where(s => s.IsDeleted == 0 && s.Recruiter.Subscriber.SubscriberGuid == guid)
                .OrderByDescending(s => s.CreateDate)
                .ToList();
        }




        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static JobPosting GetJobPostingByGuidWithRelatedObjects(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPosting
                .Include(c => c.Company)
                .Include(c => c.Industry)
                .Include(c => c.SecurityClearance)
                .Include(c => c.EmploymentType)
                .Include(c => c.ExperienceLevel)
                .Include(c => c.EducationLevel)
                .Include(c => c.CompensationType)
                .Include(c => c.JobCategory)
                .Include(c => c.Recruiter.Subscriber)
                .Include(c => c.JobPostingSkills).ThenInclude(ss => ss.Skill)
                .Where(s => s.IsDeleted == 0 && s.JobPostingGuid == guid)
                .FirstOrDefault();
        }




        /// <summary>
        /// Get a job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static JobPosting GetJobPostingByGuid(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPosting
                .Where(s => s.IsDeleted == 0 && s.JobPostingGuid == guid)
                .Include(s => s.Recruiter).ThenInclude(r => r.Subscriber)
                .Include(s => s.Company)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get an expired job posting by guid
        /// </summary>       
        /// <returns></returns>        
        public static JobPosting GetExpiredJobPostingByGuid(UpDiddyDbContext db, Guid guid)
        {
            return db.JobPosting
                .Where(s => s.IsDeleted == 1 && s.JobPostingGuid == guid)
                .FirstOrDefault();
        }


        /// <summary>
        /// Convert to a google cloude talent job object 
        /// </summary>

        /// <summary>
        /// Set default properties when a job is being added
        /// </summary>
        /// <param name="job"></param>
        public static void SetDefaultsForAddNew(JobPosting job)
        {

            job.CompanyId = -1;
            job.SecurityClearanceId = null;
            job.IndustryId = null;
            job.EmploymentTypeId = null;
            job.EducationLevelId = null;
            job.ExperienceLevelId = null;
            job.JobCategoryId = null;

        }

        public static string GetJobPostingLocation(JobPosting jobPosting)
        {
            return jobPosting.StreetAddress + " " + jobPosting.City + " " + jobPosting.Province + " " + jobPosting.PostalCode;
        }
        public static List<JobPostingSkill> GetPostingSkills(UpDiddyDbContext db, JobPosting jobPosting)
        {
            return db.JobPostingSkill
                .Include(c => c.Skill)
                .Where(s => s.IsDeleted == 0 && s.JobPostingId == jobPosting.JobPostingId)
                .ToList();
        }


        public static void UpdatePostingSkills(UpDiddyDbContext db, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {
            JobPostingSkillFactory.DeleteSkillsForPosting(db, jobPosting.JobPostingId);
            if (jobPostingDto.JobPostingSkills == null)
                return;
            foreach (SkillDto skillDto in jobPostingDto.JobPostingSkills)
            {
                JobPostingSkillFactory.Add(db, jobPosting.JobPostingId, skillDto.SkillGuid.Value);
            }
            db.SaveChanges();
        }


        /// <summary>
        /// Implement business rules to check the validity of an updated job posting 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool ValidateUpdatedJobPosting(JobPosting job, IConfiguration config, ref string message)
        {

            return ValidateJobPosting(job, config, ref message);
        }

        /// <summary>
        /// Implements business rules to check the validity of a job posting 
        /// </summary>
        /// <param name="job"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool ValidateJobPosting(JobPosting job, IConfiguration config, ref string message)
        {

            int MinDescriptionLen = int.Parse(config["CloudTalent:JobDescriptionMinLength"]);

            if (job.Description.Trim().Length < MinDescriptionLen)
            {
                message = string.Format(Constants.JobPosting.ValidationError_InvalidDescriptionLength, MinDescriptionLen);
                return false;
            }


            if (job.ThirdPartyApply == false && job.Recruiter.Subscriber == null && job.Recruiter.SubscriberId == null)
            {
                message = Constants.JobPosting.ValidationError_SubscriberRequiredMsg;
                return false;
            }

            if (job.CompanyId < 0 || job.Company == null)
            {

                message = Constants.JobPosting.ValidationError_CompanyRequiredMsg;
                return false;
            }

            if (job.ThirdPartyApply == false && job.SecurityClearance != null && job.SecurityClearanceId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidSecurityClearanceMsg;
                return false;
            }

            if (job.ThirdPartyApply == false && job.Industry != null && job.IndustryId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidIndustryMsg;
                return false;
            }

            if (job.ThirdPartyApply == false && job.JobCategory != null && job.JobCategoryId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidJobCategoryMsg;
                return false;
            }

            if (job.ThirdPartyApply == false && job.EmploymentType != null && job.EmploymentTypeId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidEmploymentTypeMsg;
                return false;
            }

            if (job.ThirdPartyApply == false && job.EducationLevel != null && job.EducationLevelId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidEducationLevelMsg;
                return false;
            }

            if (job.ThirdPartyApply == false && job.ExperienceLevel != null && job.ExperienceLevelId == null)
            {
                message = Constants.JobPosting.ValidationError_InvalidExperienceLevelMsg;
                return false;
            }

            return true;
        }

        public static void CopyPostingSkills(UpDiddyDbContext db, int sourcePostingId, int destinationPostingId)
        {
            List<JobPostingSkill> skills = JobPostingSkillFactory.GetSkillsForPosting(db, sourcePostingId);
            foreach (JobPostingSkill s in skills)
            {
                JobPostingSkillFactory.Add(db, destinationPostingId, s.Skill.SkillGuid.Value);
            }
            db.SaveChanges();
        }




        /// <summary>
        /// Save posting skills - todo use stored procedure to make this more efficient 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jobPosting"></param>
        /// <param name="jobPostingDto"></param>
        public static void SavePostingSkills(UpDiddyDbContext db, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {

            if (jobPostingDto.JobPostingSkills == null)
                return;

            foreach (SkillDto skillDto in jobPostingDto.JobPostingSkills)
            {
                JobPostingSkillFactory.Add(db, jobPosting.JobPostingId, skillDto.SkillGuid.Value);
            }
            db.SaveChanges();
        }
        /// <summary>
        /// Wire up the integer ids of all the navigation objects.   
        /// todo - find a better way to do this since it's highly unefficient.  Some options included a) exposing the dumb key via dtos 
        /// to the front end so they can pass it back and eliminate this step b) research EF to see if we can use navigation properties on GUIDS 
        /// rather than dumb int ids c) use a stored procedure to make this more efficient
        /// </summary>
        /// <param name="db"></param>
        /// <param name="jobPosting"></param>
        /// <param name="jobPostingDto"></param>
        public static void MapRelatedObjects(UpDiddyDbContext db, JobPosting jobPosting, JobPostingDto jobPostingDto)
        {

            // map subscriber 
            if (jobPostingDto.Recruiter.Subscriber != null)
            {
                Subscriber subscriber = SubscriberFactory.GetSubscriberByGuid(db, jobPostingDto.Recruiter.Subscriber.SubscriberGuid.Value);
                if (subscriber != null)
                    jobPosting.Recruiter.SubscriberId = subscriber.SubscriberId;
            }

            // map company id 
            if (jobPostingDto.Company != null)
            {
                Company company = CompanyFactory.GetCompanyByGuid(db, jobPostingDto.Company.CompanyGuid);
                if (company != null)
                    jobPosting.CompanyId = company.CompanyId;
            }
            // map industry id
            if (jobPostingDto.Industry != null)
            {
                Industry industry = IndustryFactory.GetIndustryByGuid(db, jobPostingDto.Industry.IndustryGuid);
                if (industry != null)
                    jobPosting.IndustryId = industry.IndustryId;
            }
            // map security clearance 
            if (jobPostingDto.SecurityClearance != null)
            {
                SecurityClearance securityClearance = SecurityClearanceFactory.GetSecurityClearanceByGuid(db, jobPostingDto.SecurityClearance.SecurityClearanceGuid);
                if (securityClearance != null)
                    jobPosting.SecurityClearanceId = securityClearance.SecurityClearanceId;
            }
            // map employment type
            if (jobPostingDto.EmploymentType != null)
            {
                EmploymentType employmentType = EmploymentTypeFactory.GetEmploymentTypeByGuid(db, jobPostingDto.EmploymentType.EmploymentTypeGuid);
                if (employmentType != null)
                    jobPosting.EmploymentTypeId = employmentType.EmploymentTypeId;
            }
            // map educational level type
            if (jobPostingDto.EducationLevel != null)
            {
                EducationLevel educationLevel = EducationLevelFactory.GetEducationLevelByGuid(db, jobPostingDto.EducationLevel.EducationLevelGuid);
                if (educationLevel != null)
                    jobPosting.EducationLevelId = educationLevel.EducationLevelId;
            }
            // map level experience type
            if (jobPostingDto.ExperienceLevel != null)
            {
                ExperienceLevel experienceLevel = ExperienceLevelFactory.GetExperienceLevelByGuid(db, jobPostingDto.ExperienceLevel.ExperienceLevelGuid);
                if (experienceLevel != null)
                    jobPosting.ExperienceLevelId = experienceLevel.ExperienceLevelId;
            }
            // map job category
            if (jobPostingDto.JobCategory != null)
            {
                JobCategory jobCategory = JobCategoryFactory.GetJobCategoryByGuid(db, jobPostingDto.JobCategory.JobCategoryGuid);
                if (jobCategory != null)
                    jobPosting.JobCategoryId = jobCategory.JobCategoryId;
            }

            // map compensation type 
            if (jobPostingDto.CompensationType != null)
            {
                CompensationType compensationType = CompensationTypeFactory.GetCompensationTypeByGuid(db, jobPostingDto.CompensationType.CompensationTypeGuid);
                if (compensationType != null)
                    jobPosting.CompensationTypeId = compensationType.CompensationTypeId;
            }



        }



        public static JobPosting CopyJobPosting(UpDiddyDbContext db, JobPosting jobPosting, int postingTTL)
        {

            db.Entry(jobPosting).State = EntityState.Detached;
            // use factory method to make sure all the base data values are set just 
            // in case the caller didn't set them
            BaseModelFactory.SetDefaultsForAddNew(jobPosting);
            // important! Init all reference object ids to null since further logic will use < 0 to check for 
            // their validity
            JobPostingFactory.SetDefaultsForAddNew(jobPosting);
            // assign new guid 
            jobPosting.JobPostingGuid = Guid.NewGuid();
            // null out identity column
            int SourcePostingId = jobPosting.JobPostingId;
            jobPosting.JobPostingId = 0;
            // null out skills = they will be handled explicity below
            jobPosting.JobPostingSkills = null;
            // set google index statuses 
            jobPosting.CloudTalentIndexInfo = string.Empty;
            jobPosting.CloudTalentIndexStatus = (int)GoogleCloudIndexStatus.NotIndexed;
            jobPosting.CloudTalentUri = string.Empty;
            // set posting as draft 
            jobPosting.JobStatus = (int)JobPostingStatus.Draft;
            // Set title 
            jobPosting.Title += " (copy)";
            // set expiration date 
            if (jobPosting.PostingDateUTC < DateTime.UtcNow)
                jobPosting.PostingDateUTC = DateTime.UtcNow;
            if (jobPosting.PostingExpirationDateUTC < DateTime.UtcNow)
            {
                jobPosting.PostingExpirationDateUTC = DateTime.UtcNow.AddDays(postingTTL);
            }

            db.JobPosting.Add(jobPosting);
            db.SaveChanges();
            // copy skill to new posting 
            JobPostingFactory.CopyPostingSkills(db, SourcePostingId, jobPosting.JobPostingId);

            return jobPosting;


        }

        public static bool UpdateJobPosting(UpDiddyDbContext db, Guid jobPostingGuid, JobPostingDto jobPostingDto, ref string ErrorMsg, bool isAcceptsNewSkills)
        {
            if (isAcceptsNewSkills)
            {
                var updatedSkills = new List<SkillDto>();
                foreach (var skillDto in jobPostingDto.JobPostingSkills)
                {
                    var skill = SkillFactory.GetOrAdd(db, skillDto.SkillName);
                    updatedSkills.Add(new SkillDto()
                    {
                        SkillGuid = skill.SkillGuid,
                        SkillName = skill.SkillName
                    });
                }
                jobPostingDto.JobPostingSkills = updatedSkills;
            }

            return UpdateJobPosting(db, jobPostingGuid, jobPostingDto, ref ErrorMsg);
        }

        public static bool UpdateJobPosting(UpDiddyDbContext db, Guid jobPostingGuid, JobPostingDto jobPostingDto, ref string ErrorMsg)
        {

            try
            {
                // Retreive the current state of the job posting 
                JobPosting jobPosting = JobPostingFactory.GetJobPostingByGuidWithRelatedObjects(db, jobPostingDto.JobPostingGuid.Value);
                if (jobPosting == null)
                {
                    ErrorMsg = $"{jobPostingDto.JobPostingGuid} is not a valid jobposting guid";
                    return false;
                }

                if (jobPosting.ThirdPartyApply)
                {
                    // update the recruiter information if it has changed since the last time the job page was inspected
                    RecruiterFactory.GetAddOrUpdate(db, jobPostingDto.Recruiter.Email, jobPostingDto.Recruiter.FirstName, jobPostingDto.Recruiter.LastName, jobPostingDto.Recruiter.PhoneNumber, null);
                }

                jobPosting.Title = jobPostingDto.Title;
                jobPosting.Description = jobPostingDto.Description;
                jobPosting.PostingExpirationDateUTC = jobPostingDto.PostingExpirationDateUTC;
                jobPosting.ApplicationDeadlineUTC = jobPostingDto.ApplicationDeadlineUTC;
                jobPosting.JobStatus = jobPostingDto.JobStatus;
                jobPosting.IsAgencyJobPosting = jobPostingDto.IsAgencyJobPosting;
                jobPosting.H2Visa = jobPostingDto.H2Visa;
                jobPosting.TelecommutePercentage = jobPostingDto.TelecommutePercentage;
                jobPosting.Compensation = jobPostingDto.Compensation;
                jobPosting.ThirdPartyApplicationUrl = jobPostingDto.ThirdPartyApplicationUrl;
                jobPosting.ThirdPartyApply = jobPostingDto.ThirdPartyApply;
                jobPosting.Country = jobPostingDto.Country;
                jobPosting.City = jobPostingDto.City;
                jobPosting.Province = jobPostingDto.Province;
                jobPosting.PostalCode = jobPostingDto.PostalCode;
                jobPosting.StreetAddress = jobPostingDto.StreetAddress;
                jobPosting.ThirdPartyIdentifier = jobPostingDto.ThirdPartyIdentifier;
                jobPosting.IsPrivate = jobPostingDto.IsPrivate;
                // Update the modify date to now
                jobPosting.ModifyDate = DateTime.UtcNow;

                // Map select items 
                if (jobPostingDto.Company == null)
                    jobPosting.CompanyId = null;
                else if (jobPostingDto.Company?.CompanyGuid != jobPosting.Company?.CompanyGuid)
                {
                    Company Company = CompanyFactory.GetCompanyByGuid(db, jobPostingDto.Company.CompanyGuid);
                    if (Company != null)
                        jobPosting.CompanyId = Company.CompanyId;
                    else
                        jobPosting.CompanyId = 0;
                }

                if (jobPostingDto.Industry == null)
                    jobPosting.IndustryId = null;
                else if (jobPostingDto.Industry?.IndustryGuid != jobPosting.Industry?.IndustryGuid)
                {
                    Industry industry = IndustryFactory.GetIndustryByGuid(db, jobPostingDto.Industry.IndustryGuid);
                    if (industry != null)
                        jobPosting.IndustryId = industry.IndustryId;
                    else
                        jobPosting.IndustryId = 0;
                }

                if (jobPostingDto.JobCategory == null)
                    jobPosting.JobCategoryId = null;
                else if (jobPostingDto.JobCategory?.JobCategoryGuid != jobPosting.JobCategory?.JobCategoryGuid)
                {
                    JobCategory JobCategory = JobCategoryFactory.GetJobCategoryByGuid(db, jobPostingDto.JobCategory.JobCategoryGuid);
                    if (JobCategory != null)
                        jobPosting.JobCategoryId = JobCategory.JobCategoryId;
                    else
                        jobPosting.JobCategoryId = 0;
                }

                if (jobPostingDto.SecurityClearance == null)
                    jobPosting.SecurityClearanceId = null;
                else if (jobPostingDto.SecurityClearance?.SecurityClearanceGuid != jobPosting.SecurityClearance?.SecurityClearanceGuid)
                {
                    SecurityClearance SecurityClearance = SecurityClearanceFactory.GetSecurityClearanceByGuid(db, jobPostingDto.SecurityClearance.SecurityClearanceGuid);
                    if (SecurityClearance != null)
                        jobPosting.SecurityClearanceId = SecurityClearance.SecurityClearanceId;
                    else
                        jobPosting.SecurityClearanceId = 0;
                }

                if (jobPostingDto.EmploymentType == null)
                    jobPosting.EmploymentTypeId = null;
                else if (jobPostingDto.EmploymentType?.EmploymentTypeGuid != jobPosting.EmploymentType?.EmploymentTypeGuid)
                {
                    EmploymentType EmploymentType = EmploymentTypeFactory.GetEmploymentTypeByGuid(db, jobPostingDto.EmploymentType.EmploymentTypeGuid);
                    if (EmploymentType != null)
                        jobPosting.EmploymentTypeId = EmploymentType.EmploymentTypeId;
                    else
                        jobPosting.EmploymentTypeId = 0;
                }

                if (jobPostingDto.EducationLevel == null)
                    jobPosting.EducationLevelId = null;
                else if (jobPostingDto.EducationLevel?.EducationLevelGuid != jobPosting.EducationLevel?.EducationLevelGuid)
                {
                    EducationLevel EducationLevel = EducationLevelFactory.GetEducationLevelByGuid(db, jobPostingDto.EducationLevel.EducationLevelGuid);
                    if (EducationLevel != null)
                        jobPosting.EducationLevelId = EducationLevel.EducationLevelId;
                    else
                        jobPosting.EducationLevelId = 0;
                }

                if (jobPostingDto.ExperienceLevel == null)
                    jobPosting.ExperienceLevelId = null;
                else if (jobPostingDto.ExperienceLevel?.ExperienceLevelGuid != jobPosting.ExperienceLevel?.ExperienceLevelGuid)
                {
                    ExperienceLevel ExperienceLevel = ExperienceLevelFactory.GetExperienceLevelByGuid(db, jobPostingDto.ExperienceLevel.ExperienceLevelGuid);
                    if (ExperienceLevel != null)
                        jobPosting.ExperienceLevelId = ExperienceLevel.ExperienceLevelId;
                    else
                        jobPosting.ExperienceLevelId = 0;
                }


                if (jobPostingDto.CompensationType == null)
                    jobPosting.CompensationTypeId = null;
                else if (jobPostingDto.CompensationType?.CompensationTypeGuid != jobPosting.CompensationType?.CompensationTypeGuid)
                {
                    CompensationType CompensationType = CompensationTypeFactory.GetCompensationTypeByGuid(db, jobPostingDto.CompensationType.CompensationTypeGuid);
                    if (CompensationType != null)
                        jobPosting.CompensationTypeId = CompensationType.CompensationTypeId;
                    else
                        jobPosting.CompensationTypeId = 0;
                }


                db.SaveChanges();
                JobPostingFactory.UpdatePostingSkills(db, jobPosting, jobPostingDto);
                // index active jobs in cloud talent 
                if (jobPosting.JobStatus == (int)JobPostingStatus.Active)
                {
                    // Check to see if the job has been indexed into google 
                    if (string.IsNullOrEmpty(jobPosting.CloudTalentUri) == false)
                        BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentUpdateJob(jobPosting.JobPostingGuid));
                    else
                        BackgroundJob.Enqueue<ScheduledJobs>(j => j.CloudTalentAddJob(jobPosting.JobPostingGuid));
                }
                return true;

            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                return false;

            }
        }
    }
}
