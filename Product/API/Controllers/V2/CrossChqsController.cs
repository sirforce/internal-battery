﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Business.G2;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Domain.Models.G2;
using UpDiddyLib.Domain.Models.CrossChq;

namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    [ApiController]
    public class CrossChqsController : BaseApiController
    {
        private readonly ICrosschqService _crosschqService;

        public CrossChqsController(ICrosschqService crosschqService)
        {
            _crosschqService = crosschqService;
        }

        [HttpGet("references/{profileGuid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> ReferenceRequest(Guid profileGuid)
        {
            var response = await _crosschqService.RetrieveReferenceStatus(profileGuid);

            return Ok(response);
        }

        [HttpGet("references/report/{referencecheckguid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> GetReferenceCheckReportPdf(Guid referencecheckGuid, string reportType = "Full")
        {
            var referenceCheckReport = await _crosschqService.GetReferenceCheckReportPdf(referencecheckGuid, reportType);

            return Ok(referenceCheckReport);
        }

        [HttpPost("references/{profileGuid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public async Task<IActionResult> ReferenceRequest(Guid profileGuid, [FromBody] CrossChqReferenceRequestDto referenceRequest)
        {
            var requestId = await _crosschqService
                .CreateReferenceRequest(profileGuid, GetSubscriberGuid(), referenceRequest);

            return Ok(requestId);
        }

        [HttpGet("query")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<ActionResult<CrossChqCandidateStatusListDto>> GetCrossChqStatusByResume(
            int numberOfDays,
            bool showOnlyNonCrossChq = false,
            int limit = 10,
            int offset = 0,
            string sort = "descending",
            string order = nameof(CrossChqCandidateStatusDto.ResumeUploadedDate))
        {
            var candidateStatuses = await _crosschqService
                .GetCrossChqStatusByResume(numberOfDays, showOnlyNonCrossChq, limit, offset, sort, order);

            return candidateStatuses;
        }
    }
}
