﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddy.ViewModels
{
    public class NotificationsViewModel : BaseViewModel
    {
        public int NotificationId { get; set; }
        public Guid NotificationGuid { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public bool IsTargeted { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public Guid GroupGuid { get; set; }
        public IList<GroupDto> Groups{ get; set; }
    }
}
