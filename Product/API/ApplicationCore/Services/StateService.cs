﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces;
using AutoMapper;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Exceptions;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class StateService : IStateService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;

        public StateService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
        }
        
        public async Task<StateDetailDto> GetStateDetail(Guid stateGuid)
        {
            if (stateGuid == null || stateGuid == Guid.Empty)
                throw new NullReferenceException("stateGuid cannot be null");
            var state = await _repositoryWrapper.State.GetByStateGuid(stateGuid);
            if (state == null)
                throw new NotFoundException($"State with guid: {stateGuid} does not exist");
            return _mapper.Map<StateDetailDto>(state);
        }

        public async Task<StateDetailListDto> GetStates(Guid countryGuid, int limit = 100, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            if (countryGuid == null || countryGuid == Guid.Empty)
                throw new NullReferenceException("CountryGuid cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(countryGuid);
            if (country == null)
                throw new NotFoundException("Country not found");
            var states = await _repositoryWrapper.StoredProcedureRepository.GetStates(countryGuid, limit, offset, sort, order);
            if (states == null || states.Count() == 0)
                return new StateDetailListDto() { States = new List<StateDetailDto>(), TotalRecords = 0 };
            return _mapper.Map<StateDetailListDto>(states);
        }

        public async Task<Guid> CreateState(StateDetailDto stateDetailDto)
        {
            if (stateDetailDto == null)
                throw new NullReferenceException("StateDetailDto cannot be null");
            var country = await _repositoryWrapper.Country.GetbyCountryGuid(stateDetailDto.CountryGuid);
            if (country == null)
                throw new NotFoundException("Country not found");
            var state = _mapper.Map<State>(stateDetailDto);
            state.CreateDate = DateTime.UtcNow;
            state.StateGuid = Guid.NewGuid();
            state.CountryId = country.CountryId;
            await _repositoryWrapper.State.Create(state);
            await _repositoryWrapper.SaveAsync();
            return state.StateGuid.Value;
        }

        public async Task UpdateState(StateDetailDto stateDetailDto)
        {
            if (stateDetailDto == null)
                throw new NullReferenceException("StateDetailDto cannot be null");
            var state = await _repositoryWrapper.State.GetByStateGuid(stateDetailDto.StateGuid);
            if (state == null)
                throw new NotFoundException("State not found");
            state.Name = stateDetailDto.Name;
            state.Sequence = stateDetailDto.Sequence;
            state.Code = stateDetailDto.Code;
            state.CountryId = state.CountryId;
            state.ModifyDate = DateTime.UtcNow;
            _repositoryWrapper.State.Update(state);
            await _repositoryWrapper.SaveAsync();
        }

        public async Task DeleteState(Guid stateGuid)
        {
            if (stateGuid == null || stateGuid == Guid.Empty)
                throw new NullReferenceException("StateGuid cannot be null");
            var state = await _repositoryWrapper.State.GetByStateGuid(stateGuid);
            if (state == null)
                throw new NotFoundException("State not found");
            state.IsDeleted = 1;
            _repositoryWrapper.State.Update(state);
            await _repositoryWrapper.SaveAsync();
        }
    }
}
