﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UpDiddy.ViewModels
{
    public class LinkedInSyncViewModel : BaseViewModel
    {
        public string LinkedInAvatarUrl { get; set; }
        public string SyncLink { get; set; }

        public DateTime LastLinkedInSyncDate { get; set; }
    }
}
