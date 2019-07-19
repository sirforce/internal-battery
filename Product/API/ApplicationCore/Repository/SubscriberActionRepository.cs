﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class SubscriberActionRepository : UpDiddyRepositoryBase<SubscriberAction>, ISubscriberActionRepository
    {
        public SubscriberActionRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
        }

        public async Task CreateSubscriberAction(SubscriberAction subscriberAction)
        {
            await Create(subscriberAction);
            await SaveAsync();
        }

        public async Task<List<SubscriberAction>> GetSubscriberActionByEntityAndEntityType(int entityTypeId, int? entityId = null)
        {
            IEnumerable<SubscriberAction> subscriberActionsList;
            if (entityId != null)
                subscriberActionsList = await GetByConditionAsync(sa => sa.EntityId == entityId && sa.EntityTypeId == entityTypeId && sa.IsDeleted == 0);
            else
                subscriberActionsList = await GetByConditionAsync(sa => sa.EntityTypeId == entityTypeId && sa.IsDeleted == 0);

            return subscriberActionsList.ToList();
        }
    }
}