﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public class Topic
    {
        public int TopicId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int IsDeleted { get; set; }
    }
}
