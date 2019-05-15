﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;

namespace UpDiddyApi.ApplicationCore.Repository
{
    public class UpDiddyRepositoryBase<TEntity> : IUpDiddyRepositoryBase<TEntity> where TEntity:class
    {
        private readonly UpDiddyDbContext _dbContext;

        public UpDiddyRepositoryBase(UpDiddyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IQueryable<TEntity>> GetAllAsync()
        {
            return this._dbContext
                             .Set<TEntity>()
                             .AsNoTracking();
                                
        }

        public async Task<IEnumerable<TEntity>> GetByConditionAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await this._dbContext
                             .Set<TEntity>()
                             .AsNoTracking()
                             .Where(expression)
                             .ToListAsync();
        }

        public async Task Create(TEntity entity)
        {
            await this._dbContext.Set<TEntity>().AddAsync(entity);
        }

        public void Update(TEntity entity)
        {
             this._dbContext.Set<TEntity>().Update(entity);
        }

        public void Delete(TEntity entity)
        {
            this._dbContext.Set<TEntity>().Remove(entity);
        }

        public async Task SaveAsync()
        {
            await this._dbContext.SaveChangesAsync();
        }
    }
}