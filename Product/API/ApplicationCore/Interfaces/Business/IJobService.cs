﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;
using Microsoft.AspNetCore.Http;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface IJobService
    {
        Task<List<SearchTermDto>> GetKeywordSearchTermsAsync();
        Task<List<SearchTermDto>> GetLocationSearchTermsAsync();
        Task ReferJobToFriend(JobReferralDto jobReferralDto);
        Task<Guid> ReferJobToFriend(JobReferralDto jobReferralDto, Guid subscriberGuid);
        Task UpdateJobReferral(string referrerCode, string subscriberGuid);
        Task UpdateJobViewed(string referrerCode);
        Task<JobSearchResultDto> GetJobsByLocationAsync(string Country, string Province, string City, string Industry, string JobCategory, string Skill, int PageNum,IQueryCollection query);
        Task<JobSearchSummaryResultDto> SummaryJobSearch(IQueryCollection query);
        Task<UpDiddyLib.Dto.JobPostingDto> GetJob(Guid jobPostingGuid);
        Task<JobDetailDto> GetJobDetail(Guid jobPostingGuid);
        Task<JobBrowseResultDto> BrowseJobsByLocation(IQueryCollection query);
    }
}
