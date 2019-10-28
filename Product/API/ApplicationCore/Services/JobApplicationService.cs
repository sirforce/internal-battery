﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Factory;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Exceptions;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILogger _syslog;
        private readonly UpDiddyDbContext _db = null;
        private ISubscriberService _subscriberService;
        private readonly IMapper _mapper;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IHangfireService _hangfireService;
        private readonly ISysEmail _sysEmail;
        public JobApplicationService(
            UpDiddyDbContext db,
            IRepositoryWrapper repositoryWrapper,
            ILogger<JobApplicationService> sysLog,
            IMapper mapper,
            ISubscriberService subscriberService,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IHangfireService hangfireService,
            ISysEmail sysEmail
            )
        {
            _repositoryWrapper = repositoryWrapper;
            _syslog = sysLog;
            _db = db;
            _mapper = mapper;
            _subscriberService = subscriberService;
            _configuration = configuration;
            _hangfireService = hangfireService;
            _sysEmail = sysEmail;
        }

        public async Task<bool> IsSubscriberAppliedToJobPosting(int subscriberId, int jobPostingId)
        {
            IQueryable<JobApplication> jobPosting = _repositoryWrapper.JobApplication.GetAll();
            return await jobPosting.AnyAsync(x => x.SubscriberId == subscriberId && x.JobPostingId == jobPostingId);
        }

        public async Task<bool> CreateJobApplication( Guid subscriberGuid, JobApplicationDto jobApplicationDto)
        {
            
            try
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication started at: {DateTime.UtcNow.ToLongDateString()}");
                JobPosting jobPosting = null;
                Subscriber subscriber = null;
                string ErrorMsg = string.Empty;
                int ErrorCode = 0;
                if (JobApplicationFactory.ValidateJobApplication(_db, jobApplicationDto, ref subscriber, ref jobPosting, ref ErrorCode, ref ErrorMsg) == false)
                    throw new FailedValidationException();


                // create job application 
                JobApplication jobApplication = new JobApplication();
                BaseModelFactory.SetDefaultsForAddNew(jobApplication);
                jobApplication.JobApplicationGuid = Guid.NewGuid();
                jobApplication.JobPostingId = jobPosting.JobPostingId;
                jobApplication.SubscriberId = subscriber.SubscriberId;
                jobApplication.CoverLetter = jobApplicationDto.CoverLetter == null ? string.Empty : jobApplicationDto.CoverLetter;
                // Map Partner 
                if (jobApplicationDto.Partner != null)
                {
                    PartnerType partnerType = await _repositoryWrapper.PartnerTypeRepository.GetPartnerTypeByName("ExternalSource");
                    // Get or create the referenced partner 
                    Partner partner = await _repositoryWrapper.PartnerRepository.GetOrCreatePartnerByName(jobApplicationDto.Partner.Name, partnerType);
                    jobApplication.PartnerId = partner.PartnerId;
                }

                _db.JobApplication.Add(jobApplication);
                _db.SaveChanges();

                Stream SubscriberResumeAsStream = await _subscriberService.GetResumeAsync(subscriber);
                SubscriberResumeAsStream.Seek(0, SeekOrigin.Begin);
                string resumeEncoded = Convert.ToBase64String(Utils.StreamToByteArray(SubscriberResumeAsStream));

                bool IsExternalRecruiter = jobPosting.Recruiter.Subscriber == null;

                string RecruiterEmailToUse = jobPosting.Recruiter.Subscriber?.Email ?? jobPosting.Recruiter.Email;
                // Create a jobposting dto needed for the fully qualified job posting url in the recuriter email
                JobPostingDto jobPostingDto = _mapper.Map<JobPostingDto>(jobPosting);
                // Send recruiter email alerting them to application

                Dictionary<string, bool> EmailAddressesToSend = new Dictionary<string, bool>();
                EmailAddressesToSend.Add(RecruiterEmailToUse, IsExternalRecruiter);
                var VipEmails = _configuration.GetSection("SysEmail:VIPEmails").GetChildren();

                // Ensure all VIPs get the internal email template
                foreach (IConfigurationSection Child in VipEmails)
                {
                    string VipEmail = Child.Value;
                    EmailAddressesToSend.Add(VipEmail, false);
                }

                foreach (string Email in EmailAddressesToSend.Keys)
                {
                    _hangfireService.Enqueue(() => _sysEmail.SendTemplatedEmailAsync
                    (
                        Email,
                        _configuration["SysEmail:Transactional:TemplateIds:JobApplication-Recruiter" +
                            (EmailAddressesToSend[Email] == true ? "-External" : string.Empty)],
                        new
                        {
                            ApplicantName = jobApplicationDto.Subscriber.FirstName + " " + jobApplicationDto.Subscriber.LastName,
                            ApplicantFirstName = jobApplicationDto.Subscriber.FirstName,
                            ApplicantLastName = jobApplicationDto.Subscriber.LastName,
                            ApplicantEmail = subscriber.Email,
                            JobTitle = jobPosting.Title,
                            ApplicantUrl = SubscriberFactory.JobseekerUrl(_configuration, subscriber.SubscriberGuid.Value),
                            JobUrl = JobPostingFactory.JobPostingFullyQualifiedUrl(_configuration, jobPostingDto),
                            Subject = (IsExternalRecruiter == true ? $"{jobPosting.Company.CompanyName} job posting via CareerCircle" : "Applicant Alert"),
                            RecruiterGuid = jobPosting.Recruiter.RecruiterGuid,
                            JobApplicationGuid = jobApplication.JobApplicationGuid
                        },
                        Constants.SendGridAccount.Transactional,
                        null,
                        new List<Attachment>
                        {
                            new Attachment
                            {
                                Content = resumeEncoded,
                                Filename = Path.GetFileName(subscriber.SubscriberFile.FirstOrDefault().BlobName),
                                Type=subscriber.SubscriberFile.FirstOrDefault().MimeType
                            },
                            new Attachment
                            {
                                Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jobApplicationDto.CoverLetter)),
                                Filename = "CoverLetter.txt"
                            }
                        },
                        null,
                        null
                    ));
                }


                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication completed at: {DateTime.UtcNow.ToLongDateString()}");
                return true;
            }
            catch (Exception ex)
            {
                _syslog.Log(LogLevel.Information, $"***** JobApplicationController:CreateJobApplication exception : {ex.Message}");
                throw new Exception($"***** JobApplicationController:CreateJobApplication exception : {ex.Message}");
            }
        }

 
 
    }





}
 