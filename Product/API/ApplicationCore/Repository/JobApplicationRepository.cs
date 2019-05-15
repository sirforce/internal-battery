using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class JobApplicationRepository :UpDiddyRepositoryBase<JobApplication>, IJobApplicationRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public JobApplicationRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<IQueryable<JobApplication>> GetAllJobApplicationsAsync()
        {
           return GetAllAsync();
        }
    }
}
