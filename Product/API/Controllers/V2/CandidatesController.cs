﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.Candidate360;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class CandidatesController : BaseApiController
    {
        private readonly ICandidatesService _candidatesService;
        private readonly ILogger _logger;
 

        public CandidatesController(
            ICandidatesService candidatesService,
            ILogger<CandidatesController> logger
            )
        {
            _candidatesService = candidatesService;
            _logger = logger;
        }

        #region Personal Info

        [HttpGet]
        [Route("personal-information")]
        public async Task<IActionResult> GetCandidatePersonalInfo()
        {
            var response = await _candidatesService.GetCandidatePersonalInfo(GetSubscriberGuid());
            return Ok(response);
        }

        [HttpPut]
        [Route("personal-information")]
        public async Task<IActionResult> UpdateCandidatePersonalInfo([FromBody] CandidatePersonalInfoDto request)
        {
            await _candidatesService.UpdateCandidatePersonalInfo(GetSubscriberGuid(), request);
            return StatusCode(204);
        }
        #endregion Personal Info

        #region Employment Preferences


        [HttpGet]
        [Route("employment-preferences")]
        public async Task<IActionResult> GetCandidateEmploymentPreference()
        {
            var response = await _candidatesService.GetCandidateEmploymentPreference(GetSubscriberGuid());
            return Ok(response);
        }

        [HttpPut]
        [Route("employment-preferences")]
        public async Task<IActionResult> UpdateCandidateEmploymentPreference([FromBody] CandidateEmploymentPreferenceDto request)
        {
            await _candidatesService.UpdateCandidateEmploymentPreference(GetSubscriberGuid(), request);
            return StatusCode(204);
        }



        #endregion Employment Preferences

        #region Role Preferences

        [HttpGet("role-preferences")]
        [Authorize]
        public async Task<ActionResult<RolePreferenceDto>> GetRolePreferenceDto()
        {
            var subscriberGuid = GetSubscriberGuid();

            var rolePreferenceDto = await _candidatesService.GetRolePreference(subscriberGuid);
            return rolePreferenceDto;
        }

        [HttpPut("role-preferences")]
        [Authorize]
        public async Task<IActionResult> UpdateRolePreferenceDto(RolePreferenceDto rolePreference)
        {
            var subscriberGuid = GetSubscriberGuid();

            await _candidatesService.UpdateRolePreference(subscriberGuid, rolePreference);
            return StatusCode(204);
        }

        #endregion Role Preferences

        #region Skills 

        [HttpGet("skills")]
        [Authorize]
        public async Task<ActionResult<SkillListDto>> GetSkills(int limit = 10, int offset = 0, string sort = "name", string order = "ascending")
        {
            return Ok(await _candidatesService.GetSkills(GetSubscriberGuid(), limit, offset, sort, order));
        }

        [HttpPut("skills")]
        [Authorize]
        public async Task<IActionResult> UpdateSkills([FromBody] List<string> skillNames)
        {
            await _candidatesService.UpdateSkills(GetSubscriberGuid(), skillNames);
            return StatusCode(204);
        }

        #endregion

        #region Language Proficiencies

        [HttpGet("languages")]
        public async Task<ActionResult<LanguageListDto>> GetLanguageList()
            => await _candidatesService.GetLanguageList();

        [HttpGet("language-proficiency-levels")]
        public async Task<ActionResult<ProficiencyLevelListDto>> GetProficiencyLevelList()
            => await _candidatesService.GetProficiencyLevelList();

        [HttpGet("language-proficiency")]
        [Authorize]
        public async Task<ActionResult<LanguageProficiencyListDto>> GetLanguageProficiencies()
        {
            var subscriberGuid = base.GetSubscriberGuid();
            var languagesAndProficiencies = await _candidatesService.GetLanguageProficiencies(subscriberGuid);

            return languagesAndProficiencies;
        }

        [HttpPost("language-proficiency")]
        [Authorize]
        public async Task<IActionResult> CreateLanguageProficiency([FromBody] LanguageProficiencyDto languageProficiency)
        {
            var subscriberGuid = base.GetSubscriberGuid();
            var languageproficiencyguid = await _candidatesService.CreateLanguageProficiency(languageProficiency, subscriberGuid);

            return StatusCode(201, languageproficiencyguid);
        }

        [HttpPut("language-proficiency/{languageproficiencyguid}")]
        [Authorize]
        public async Task<IActionResult> UpdateLanguageProficiency(Guid languageProficiencyGuid, [FromBody] LanguageProficiencyDto languageProficiency)
        {
            var subscriberGuid = base.GetSubscriberGuid();
            await _candidatesService.UpdateLanguageProficiency(languageProficiency, languageProficiencyGuid, subscriberGuid);

            return NoContent();
        }

        [HttpDelete("language-proficiency/{languageproficiencyguid}")]
        [Authorize]
        public async Task<IActionResult> DeleteLanguageProficiency(Guid languageProficiencyGuid)
        {
            var subscriberGuid = base.GetSubscriberGuid();
            await _candidatesService.DeleteLanguageProficiency(languageProficiencyGuid, subscriberGuid);

            return NoContent();
        }

        #endregion Language Proficiencies

        #region CompensationPreferences

        [HttpGet("compensation-preferences")]
        [Authorize]
        public async Task<ActionResult<CompensationPreferencesDto>> GetCompensationPreferences()
        {
            var subscriberGuid = GetSubscriberGuid();
            var compensationPreferences = await _candidatesService.GetCompensationPreferences(subscriberGuid);
            return compensationPreferences;
        }

        [HttpPut("compensation-preferences")]
        [Authorize]
        public async Task<IActionResult> UpdateCompensationPreferences([FromBody] CompensationPreferencesDto compensationPreferences)
        {
            var subscriberGuid = GetSubscriberGuid();
            await _candidatesService.UpdateCompensationPreferences(compensationPreferences, subscriberGuid);
            return NoContent();
        }

        #endregion CompensationPreferences

        #region Education & Assessments
        
        [HttpGet]
        [Route("education-history")]
        [Authorize]
        public async Task<IActionResult> GetCandidateEducationHistory(int limit = 10, int offset = 0, string sort = "createdate", string order = "ascending")
        {
            var response = await _candidatesService.GetCandidateEducationHistory(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(response);
        }

        [HttpGet]
        [Route("educational-degrees")]
        public async Task<IActionResult> GetAllEducationalDegrees(int limit = 10, int offset = 0, string sort = "sequence", string order = "ascending")
        {
            var response = await _candidatesService.GetAllEducationalDegrees(limit, offset, sort, order);
            return Ok(response);
        }

        [HttpGet]
        [Route("training-history")]
        [Authorize]
        public async Task<IActionResult> GetCandidateTrainingHistory(int limit = 10, int offset = 0, string sort = "createdate", string order = "ascending")
        {
            var response = await _candidatesService.GetCandidateTrainingHistory(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(response);
        }

        [HttpGet]
        [Route("training-types")]
        public async Task<IActionResult> GetAllTrainingTypes(int limit = 10, int offset = 0, string sort = "sequence", string order = "ascending")
        {
            var response = await _candidatesService.GetAllTrainingTypes(limit, offset, sort, order);
            return Ok(response);
        }

        [HttpPut]
        [Authorize]
        [Route("education-training-history")]
        public async Task<IActionResult> UpdateCandidateEducationAndTraining([FromBody] SubscriberEducationAssessmentsDto request)
        {
            await _candidatesService.UpdateCandidateEducationAndTraining(GetSubscriberGuid(), request);
            return StatusCode(204);
        }

        [HttpGet]
        [Authorize]
        [Route("assessments")]
        public async Task<IActionResult> GetAssessments()
        {
            var response = await _candidatesService.GetAssessments(GetSubscriberGuid());
            return Ok(response);
        }

        #endregion Education & Assessments

        #region Candidate Indexing Operations

        /// <summary>
        /// Re-index subsriber.  This operation will update as well as create documents in the 
        /// azure candidate index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("index/subscriber/{subscriberGuid}")]
        public async Task<IActionResult> IndexSubscriber(Guid subscriberGuid)
        {
 
            _candidatesService.IndexCandidateBySubscriberAsync(subscriberGuid, true);
            return StatusCode(202);
        }


        /// <summary>
        /// This operation will remove the subscriber from the azure candidate index
        /// azure candidate index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("index/subscriber/{subscriberGuid}")]
        public async Task<IActionResult> IndexRemoveSubscriber(Guid subscriberGuid)
        {
           
            _candidatesService.IndexRemoveCandidateBySubscriberAsync(subscriberGuid, true);
            return StatusCode(202);
        }


        /// <summary>
        /// Re-index subsriber.  This operation will update as well as create documents in the 
        /// azure candidate index 
        /// </summary>
        /// <param name="subscriberGuid"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("index")]
        public async Task<IActionResult> IndexAllUnindexedSubscriber()
        {
            _candidatesService.IndexAllUnindexed(true);
            return StatusCode(202);
        }




        #endregion

        #region Work History

        [HttpGet]
        [Authorize]
        [Route("work-history")]
        public async Task<IActionResult> GetCandidateWorkHistory(int limit = 10, int offset = 0, string sort = "createdate", string order = "ascending")
        {
            var response = await _candidatesService.GetCandidateWorkHistory(GetSubscriberGuid(), limit, offset, sort, order);
            return Ok(response);
        }

        [HttpPut]
        [Authorize]
        [Route("work-history")]
        public async Task<IActionResult> UpdateCandidateWorkHistory([FromBody] WorkHistoryUpdateDto request)
        {
            await _candidatesService.UpdateCandidateWorkHistory(GetSubscriberGuid(), request);
            return StatusCode(204);
        }

        #endregion
    }
}
