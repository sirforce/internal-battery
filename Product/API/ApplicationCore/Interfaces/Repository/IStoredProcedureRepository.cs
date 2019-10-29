using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IStoredProcedureRepository
    {
        Task<List<SearchTermDto>> GetKeywordSearchTermsAsync();
        Task<List<SearchTermDto>> GetLocationSearchTermsAsync();
        Task<List<JobAbandonmentStatistics>> GetJobAbandonmentStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<List<JobCountPerProvince>> GetJobCountPerProvince();
        Task<List<JobDto>> GetSubscriberJobFavorites(int SubscriberId);
        Task<List<SubscriberSourceDto>> GetSubscriberSources(int SubscriberId);
        Task<List<SubscriberSignUpCourseEnrollmentStatistics>> GetSubscriberSignUpCourseEnrollmentStatisticsAsync(DateTime? startDate, DateTime? endDate);
        Task<int> AddOrUpdateCourseAsync(CourseParams courseParams);
        Task<List<SubscriberInitialSourceDto>> GetNewSubscribers();

        Task<List<CourseDetailDto>> GetCoursesForJob(Guid JobGuid, int NumCourses);
        Task<List<CourseDetailDto>> GetCoursesBySkillHistogram(Dictionary<string, int> SkillHistogram, int NumCourses);
        Task<List<CourseDetailDto>> GetCoursesRandom(int NumCourses);
    }
}