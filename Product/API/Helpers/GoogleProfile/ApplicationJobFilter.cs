﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class ApplicationJobFilter
    {
        public string jobRequisitionId { get; set; }
        public string jobTitle { get; set; }
        public bool negated { get; set; }

    }
}
