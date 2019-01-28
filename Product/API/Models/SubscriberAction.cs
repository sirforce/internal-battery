﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class SubscriberAction : BaseModel
    {
        public Guid? SubscriberActionGuid { get; set; }
        public int SubscriberId { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public int ActionId { get; set; }
        public virtual Action Action { get; set; }
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public DateTime OccurredDate { get; set; }
    }
}
