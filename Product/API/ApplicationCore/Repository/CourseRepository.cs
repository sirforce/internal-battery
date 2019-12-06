﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Repository
{
    public class CourseRepository : UpDiddyRepositoryBase<Course>, ICourseRepository
    {
        private readonly UpDiddyDbContext _dbContext;
        public CourseRepository(UpDiddyDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Course>> GetCoursesByTopicGuid(Guid topicGuid)
        {
            return await (from t in _dbContext.Topic
                          join c in _dbContext.Course on t.TopicId equals c.TopicId
                          where c.IsDeleted == 0 && t.IsDeleted == 0
                          select c).Include(x => x.Vendor).OrderBy(x =>x.SortOrder).ToListAsync();
        }

        public async Task<int> GetCoursesCount()
        {
            return await _dbContext.Course.Where(x => x.IsDeleted == 0).CountAsync();
        }
    }
}
