﻿using System;
using System.Collections.Generic;
using UpDiddyApi.Models;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ITrackingService
    {
        Task AddUpdateLandingPageTracking(string url);
        Task RecordSubscriberApplyActionAsync(Guid jobGuid, Guid subscriberGuid);
        Task<Dictionary<Subscriber, List<JobPosting>>> GetSubscriberAbandonedJobPostingHistoryByDateAsync(DateTime datetime);
        Task<UrlDto> GetFullUrlAfterTracking(string slugName);
        Task TrackingSubscriberJobViewAction(Guid jobGuid, Guid subscriberGuid);
        Task TrackingSubscriberFileDownloadAction(int subscriberId, int fileDownloadTrackerId);
        Task TrackSubscriberAction(int subscriberId, Models.Action action, EntityType entityType, int entityId);
    }
}
