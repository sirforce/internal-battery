﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class JobPostingFavoriteDto
    {

        public Guid JobPostingFavoriteGuid { get; set; } 
        public virtual JobPostingDto JobPosting { get; set; } 
        public virtual SubscriberDto Subscriber { get; set; }
    }
}
