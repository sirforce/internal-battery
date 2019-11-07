﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class StateRepository : UpDiddyRepositoryBase<State>, IStateRepository
    {


        private readonly UpDiddyDbContext _dbContext;

        public StateRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<State> GetStateBySubscriberGuid(Guid subscriberGuid)
        {
            return await (from s in _dbContext.State join
                          su in _dbContext.Subscriber on s.StateId equals su.StateId
                          where su.SubscriberGuid == subscriberGuid
                          select s).FirstOrDefaultAsync();
        }


        public async Task<IEnumerable<State>> GetAllStatesAsync()
        {
            var states = GetAll();
            return await states
                               .Include(s => s.Country)
                               .ToListAsync();                             
        }

        public async Task<IEnumerable<State>> GetStatesByCountryGuid(Guid countryGuid)
        {
            var states = GetAll();
            return await states
                                .Include(s => s.Country)
                                .Where(s=>s.IsDeleted==0 && s.Country.CountryGuid== countryGuid)
                                .ToListAsync();
        }

        public async Task<IEnumerable<State>> GetStatesForDefaultCountry()
        {
            var states = GetAll();
            return await states
                               .Include(s => s.Country)
                               .Where(s => s.IsDeleted == 0 && s.Country.Sequence == 1)
                               .OrderBy(s=>s.Sequence)
                               .ToListAsync();
        }
    }
}
