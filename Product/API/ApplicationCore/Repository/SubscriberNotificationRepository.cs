﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public class SubscriberNotificationRepository : UpDiddyRepositoryBase<SubscriberNotification>, ISubscriberNotificationRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public SubscriberNotificationRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SubscriberNotification> GetSubscriberNotificationByIdentifiersAsync(Guid subscriberGuid, Guid subscriberNotificationGuid)
        {
            return await _dbContext.SubscriberNotification.IgnoreQueryFilters()
                .Where(sn => sn.Subscriber.SubscriberGuid == subscriberGuid && sn.SubscriberNotificationGuid == subscriberNotificationGuid)
                .FirstOrDefaultAsync();
        }
    }
}
