﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class JobPostingService : IJobPostingService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        public JobPostingService(IRepositoryWrapper repositoryWrapper) => _repositoryWrapper = repositoryWrapper;
        /// <summary>
        /// Gets the job count based on state (province). It utilizes two types of enums (state prefix and state name)
        /// because the jobposting's province column has data that spells out state names and that uses abbreviations.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<JobPostingCountDto>> GetJobCountPerProvinceAsync()
        {
            var query = _repositoryWrapper.JobPosting.GetAll();
            var jobCount = new List<JobPostingCountDto>();
            Enums.ProvincePrefix statePrefixEnum;
            Enums.ProvinceName stateNameEnum;
            var provinceList = await query
                .Where(x => x.IsDeleted == 0)
                .Select(x => x.Province)
                .Distinct()
                .ToListAsync();
            foreach (var province in provinceList)
            {
                var str = province.Trim().Replace(" ", "").ToUpper();        
                string statePrefix = string.Empty;
                if (Enum.TryParse(str, out statePrefixEnum))
                {
                    statePrefix = str;
                }
                else if (Enum.TryParse(str, out stateNameEnum))
                {         
                   Enums.ProvinceName value = (Enums.ProvinceName)(int)stateNameEnum;
                   statePrefix = value.ToString();
                }
                if (!String.IsNullOrEmpty(statePrefix))
                {
                    var companyQuery = await GetJobsByStateQuery(province);
                    if (companyQuery.Count > 0)
                    {
                        var total = companyQuery.Sum(c => c.JobCount);
                        jobCount.Add(new JobPostingCountDto(statePrefix, companyQuery, total));
                    }
                }
            }
            return jobCount;
        }

        private async Task<List<JobPostingCompanyCountDto>> GetJobsByStateQuery(string province)
        {
            var query = _repositoryWrapper.JobPosting.GetAll();
            return await query.Where(x => x.Province == province && x.IsDeleted == 0)
                .GroupBy(l => l.Company)
                .Select(g => new JobPostingCompanyCountDto()
                {
                    CompanyGuid = g.Key.CompanyGuid,
                    CompanyName = g.Key.CompanyName,
                    JobCount = g.Distinct().Count()
                })
                .OrderByDescending(x => x.JobCount)
                .Take(3)
                .ToListAsync();
        }
    }
}