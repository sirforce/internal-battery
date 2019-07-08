﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IRepositoryWrapper
    {
        ICountryRepository Country { get; }
        IStateRepository State { get; }
        IJobSiteRepository JobSite { get; }
        IJobPageRepository JobPage { get; }
        IJobSiteScrapeStatisticRepository JobSiteScrapeStatistic { get; }
        IJobCategoryRepository JobCategoryRepository { get; }
        IJobPostingRepository JobPosting { get; }
        IJobPostingFavoriteRepository JobPostingFavorite { get; }
        IJobApplicationRepository JobApplication { get; }
        ICompanyRepository Company { get; }
        IRecruiterActionRepository RecruiterActionRepository { get; }
        ISubscriberRepository Subscriber { get; }
        IZeroBounceRepository ZeroBounceRepository { get; }
        IPartnerContactLeadStatusRepository PartnerContactLeadStatusRepository { get; }
        ISubscriberRepository SubscriberRepository { get; }
        IJobReferralRepository JobReferralRepository { get; }
        ISubscriberNotesRepository SubscriberNotesRepository { get; }
        IRecruiterRepository RecruiterRepository { get; }
        INotificationRepository NotificationRepository { get; }
        ISubscriberNotificationRepository SubscriberNotificationRepository { get; }
        IJobPostingAlertRepository JobPostingAlertRepository { get; }
        IResumeParseRepository ResumeParseRepository { get; }
        IResumeParseResultRepository ResumeParseResultRepository { get; }
        IPartnerReferrerRepository PartnerReferrerRepository { get; }
        IGroupPartnerRepository GroupPartnerRepository { get; }
        IGroupRepository GroupRepository { get; }
        ISubscriberGroupRepository SubscriberGroupRepository { get; }
        IPartnerContactRepository PartnerContactRepository { get; }
    }
}