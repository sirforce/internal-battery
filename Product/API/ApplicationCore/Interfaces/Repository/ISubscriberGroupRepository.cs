﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface ISubscriberGroupRepository : IUpDiddyRepositoryBase<SubscriberGroup>
    {
        Task<IList<Subscriber>> GetSubscribersAssociatedWithGroupAsync(int GroupId);
 
    }
    
}
