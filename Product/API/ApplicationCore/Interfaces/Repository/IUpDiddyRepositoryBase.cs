﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IUpDiddyRepositoryBase<TEntity> where TEntity: class
    {
        Task<IQueryable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetByConditionAsync(Expression<Func<TEntity,bool>> expression);
        Task Create(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task SaveAsync();
    }
}