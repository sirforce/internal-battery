﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
   public class CompanyDto 
    {
      
        public Guid CompanyGuid { get; set; }
        public string CompanyName { get; set; }
        public string LogoUrl { get; set; }
        public int IsHiringAgency { get; set; }
        public int IsJobPoster { get; set; }
    }
}
