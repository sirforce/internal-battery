﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class CourseVariantPromoCode : BaseModel
    {
        public int CourseVariantPromoCodeId { get; set; }
        public Guid? CourseVariantPromoCodeGuid { get; set; }
        public int CourseVariantId { get; set; }
        public int PromoCodeId { get; set; }
        public int? MaxAllowedNumberOfRedemptions { get; set; }
        public int NumberOfRedemptions { get; set; }
    }
}
