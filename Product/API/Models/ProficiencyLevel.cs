﻿using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyApi.Models
{
    public class ProficiencyLevel : BaseModel
    {
        public int ProficiencyLevelId { get; set; }

        public Guid ProficiencyLevelGuid { get; set; }

        [Required]
        [StringLength(500)]
        public string ProficiencyLevelName { get; set; }
    }
}
