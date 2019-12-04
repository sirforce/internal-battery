﻿using System.Threading.Tasks;
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
        IPartnerRepository PartnerRepository { get; }
        ISubscriberActionRepository SubscriberActionRepository { get; }
        IEntityTypeRepository EntityTypeRepository { get; }
        IActionRepository ActionRepository { get; }
        IContactRepository ContactRepository { get; }
        IOfferRepository Offer { get; }
        ISubscriberFileRepository SubscriberFileRepository { get; }
        ISkillRepository SkillRepository { get; }
        IStoredProcedureRepository StoredProcedureRepository { get; }
        ICourseSiteRepository CourseSite { get; }
        ICoursePageRepository CoursePage { get; }
        ICourseRepository Course { get; }
        ICourseSkillRepository CourseSkill { get; }
        ICourseVariantRepository CourseVariant { get; }
        ICourseVariantTypeRepository CourseVariantType { get; }
        ITagRepository Tag { get; }
        ITagTopicRepository TagTopic { get; }
        ITopicRepository Topic { get; }
        ITagCourseRepository TagCourse { get; }
        IVendorRepository Vendor { get; }
        IEnrollmentRepository EnrollmentRepository { get; }
        ITraitifyRepository TraitifyRepository { get; }
        IServiceOfferingRepository ServiceOfferingRepository { get; }
        IServiceOfferingItemRepository ServiceOfferingItemRepository { get; }
        IServiceOfferingOrderRepository ServiceOfferingOrderRepository { get; }
        IServiceOfferingPromoCodeRedemptionRepository ServiceOfferingPromoCodeRedemptionRepository { get; }
        IServiceOfferingPromoCodeRepository ServiceOfferingPromoCodeRepository { get; }
        IPromoCodeRepository PromoCodeRepository { get; }
        IFileDownloadTrackerRepository FileDownloadTrackerRepository { get; }
        IPartnerTypeRepository PartnerTypeRepository { get; }
        IJobPostingSkillRepository JobPostingSkillRepository { get; }
        ICampaignPartnerContactRepository CampaignPartnerContactRepository { get; }
        ICampaignRepository CampaignRepository { get; }
        ISubscriberWorkHistoryRepository SubscriberWorkHistoryRepository { get; }
        ISubscriberSkillRepository SubscriberSkillRepository { get; }
        ISubscriberEducationHistoryRepository SubscriberEducationHistoryRepository { get; }
        IIndustryRepository IndustryRepository { get; }
        ISecurityClearanceRepository SecurityClearanceRepository { get; }
        IEmploymentTypeRepository EmploymentTypeRepository { get; }
        IEducationalDegreeRepository EducationalDegreeRepository { get; }
        IEducationalDegreeTypeRepository EducationalDegreeTypeRepository { get; }
        IEducationalInstitutionRepository EducationalInstitutionRepository { get; }
        IEducationLevelRepository EducationLevelRepository { get; }
        IExperienceLevelRepository ExperienceLevelRepository { get; }
        ICompensationTypeRepository CompensationTypeRepository { get; }
        IRecruiterCompanyRepository RecruiterCompanyRepository { get; }
        Task SaveAsync();
        ITraitifyCourseTopicBlendMappingRepository TraitifyCourseTopicBlendMappingRepository { get; }
        ICourseFavoriteRepository CourseFavoriteRepository { get; }
        ISubscriberProfileStagingStoreRepository SubscriberProfileStagingStoreRepository { get; }
        ITalentFavoriteRepository TalentFavoriteRepository { get; }
        ICareerPathCourseRepository CareerPathCourseRepository { get; }
        ICareerPathRepository CareerPathRepository { get; }
    }
}
