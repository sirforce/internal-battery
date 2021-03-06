﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class AzureIndexStatusRepository : UpDiddyRepositoryBase<AzureIndexStatus>, IAzureIndexStatusRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public AzureIndexStatusRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AzureIndexStatus> GetAzureIndexStatusByName(string name)
        {
            var status =  _dbContext.AzureIndexStatus
                              .Where(s => s.IsDeleted == 0 && s.Name == name)
                              .FirstOrDefault();
            
            return status;
        }


    }
}
