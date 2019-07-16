﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class GroupPartnerRepository : UpDiddyRepositoryBase<GroupPartner>, IGroupPartnerRepository
    {
        public GroupPartnerRepository(UpDiddyDbContext dbContext) : base(dbContext) { }
    }
}
