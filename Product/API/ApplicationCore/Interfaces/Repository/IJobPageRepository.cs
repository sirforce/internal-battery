﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IJobPageRepository : IUpDiddyRepositoryBase<JobPage>
    {
        Task<IEnumerable<JobPage>> GetAllJobPagesForJobSiteAsync(Guid jobSiteGuid);
    }
}
