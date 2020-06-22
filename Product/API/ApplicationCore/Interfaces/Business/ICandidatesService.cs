﻿using System;
using System.Threading.Tasks;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface ICandidatesService
    {

        Task<CandidateEmploymentPreferenceDto> GetCandidateEmploymentPreference(Guid subscriberGuid);
        Task<CandidatePersonalInfoDto> GetCandidatePersonalInfo(Guid subscriberGuid);
        Task UpdateCandidateEmploymentPreference(Guid subscriberGuid, CandidateEmploymentPreferenceDto candidateEmploymentPreferenceDto);
        Task UpdateCandidatePersonalInfo(Guid subscriberGuid, CandidatePersonalInfoDto candidatePersonalInfoDto);

        Task<RolePreferenceDto> GetRolePreference(Guid subscriberGuid);

        Task UpdateRolePreference(Guid subscriberGuid, RolePreferenceDto rolePreference);
    }
}
