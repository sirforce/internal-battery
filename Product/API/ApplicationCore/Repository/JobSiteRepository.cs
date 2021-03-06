﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobSiteRepository : UpDiddyRepositoryBase<JobSite>, IJobSiteRepository
    {
        
        public JobSiteRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {

        }

        public async Task<IEnumerable<JobSite>> GetAllJobSitesAsync()
        {
            var jobSites = GetAll();
            return await jobSites
                .Where(js => js.IsDeleted == 0)
                .OrderBy(js => js.JobSiteId)
                .ToListAsync();
        }
    }
}
