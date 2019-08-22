﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IGroupPartnerRepository : IUpDiddyRepositoryBase<GroupPartner> 
    {
        Task<GroupPartner> GetGroupPartnerByGroupIdPartnerId(int Groupid, int PartnerId);
    }
}
