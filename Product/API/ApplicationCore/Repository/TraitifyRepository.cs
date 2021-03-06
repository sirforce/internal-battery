﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Repository
{

    public class TraitifyRepository : UpDiddyRepositoryBase<Traitify>, ITraitifyRepository
    {
        UpDiddyDbContext _dbContext;

        public TraitifyRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Traitify> GetByAssessmentId(string assessmentId)
        {
            return await _dbContext.Traitify.Where(x => x.AssessmentId == assessmentId).FirstOrDefaultAsync();
        }

        public async Task<Traitify> GetMostRecentAssessmentBySubscriber(Guid subscriber)
        {
            return await _dbContext.Traitify.Where(t => t.Subscriber.SubscriberGuid == subscriber && t.IsDeleted == 0 && !string.IsNullOrWhiteSpace(t.ResultData)).OrderByDescending(t => t.CreateDate).FirstOrDefaultAsync();
        }
    }
}
