﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UpDiddyLib.Domain.Models.CrossChq
{
    public class ReferenceResponseDto
    {
        [JsonProperty("id")]
        public string ReferenceResponseId { get; set; }

        [JsonProperty("candidate")]
        public ReferenceCandidateDto Candidate { get; set; }

        [JsonProperty("job_position")]
        public string JobPosition { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_updated_on")]
        public DateTime? StatusUpdatedOn { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }

        [JsonProperty("references")]
        public List<ReferenceDto> References { get; set; }

        [JsonProperty("report_url")]
        public string ReportURL { get; set; }

        [JsonProperty("report_summary_pdf")]
        public string ReportSummaryPDF { get; set; }

        [JsonProperty("report_full_pdf")]
        public string ReportFullPDF { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_on")]
        public DateTime? UpdatedOn { get; set; }
    }
}
