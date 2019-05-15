﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IStateRepository : IUpDiddyRepositoryBase<State>
    {
        Task<IEnumerable<State>> GetAllStatesAsync();
        Task<IEnumerable<State>> GetStatesByCountryGuid(Guid countryGuid);
        Task<IEnumerable<State>> GetStatesForDefaultCountry();
    }
}