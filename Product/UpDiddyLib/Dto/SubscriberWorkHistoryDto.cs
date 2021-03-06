﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class SubscriberWorkHistoryDto : BaseDto
    {
        public int SubscriberWorkHistoryId { get; set; }
        public Guid SubscriberWorkHistoryGuid { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int IsCurrent { get; set; }
        public string Title { get; set; }
        public string JobDescription { get; set; }
        public decimal Compensation { get; set; }
        public string CompensationType { get; set; }
        public Guid CompensationTypeGuid { get; set; }
        public string Company { get; set; }
    }
}
