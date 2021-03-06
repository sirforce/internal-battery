﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Campaign : BaseModel
    {
        public int CampaignId { get; set; }
        [Required]
        public Guid CampaignGuid { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<CampaignCourseVariant> CampaignCourseVariant { get; set; } = new List<CampaignCourseVariant>();
        public List<CampaignPartnerContact> CampaignPartnerContact { get; set; } = new List<CampaignPartnerContact>();
        public string Terms { get; set; }
        public string TargetedViewName { get; set; }
    }
}