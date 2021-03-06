﻿using System;
using System.Collections.Generic;
using UpDiddyApi.Models;
using System.Threading.Tasks;
using UpDiddyLib.Dto.Reporting;
using Microsoft.AspNet.OData.Query;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IReportingService
    {
        Task<List<JobApplicationCountDto>> GetApplicationCountByCompanyAsync(ODataQueryOptions<JobApplication> query, Guid? companyGuid);
        Task<List<JobPostingCountReportDto>> GetActiveJobPostCountPerCompanyByDates(DateTime? startPostDate, DateTime? endPostDate);
        Task<List<JobViewCountDto>> GetJobViewCount(Guid jobPostingGuid);
        Task<List<NotificationCountsReportDto>> GetReadNotificationsAsync(ODataQueryOptions<Notification> query);
        Task<List<JobAbandonmentStatistics>> GetJobAbandonmentCountByDateAsync(DateTime startDate, DateTime endDate);
        Task<List<SubscriberSignUpCourseEnrollmentStatistics>> GetSubscriberSignUpCourseEnrollmentStatisticsAsync(DateTime? startDate, DateTime? endDate);
        Task<SubscriberReportDto> GetSubscriberAndEnrollmentReportByDates(List<DateTime> dates);
    }
}
