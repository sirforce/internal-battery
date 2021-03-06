﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models.B2B
{
    [Table("HiringManagers", Schema = "B2B")]
    public class HiringManager : BaseModel
    {
        public int HiringManagerId { get; set; }
        public Guid HiringManagerGuid { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public string HardToFindFillSkillsRoles { get; set; }
        public string SkillsRolesWeAreAlwaysHiringFor { get; set; }
    }
}