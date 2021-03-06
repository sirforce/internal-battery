﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models.B2B
{
    public class PipelineProfileListDto
    {
        public List<PipelineProfileDto> PipelineProfiles { get; set; } = new List<PipelineProfileDto>();
        public int TotalRecords { get; set; }
    }

    public class PipelineProfileDto
    {
        public Guid PipelineProfileGuid { get; set; }
        public Guid ProfileGuid { get; set; }
        public Guid HiringManagerGuid { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Title { get; set; }
        [JsonIgnore]
        public int TotalRecords { get; set; }
    }
}
