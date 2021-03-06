﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UpDiddyApi.ApplicationCore.Interfaces.Repository
{
    public interface IUpDiddyRepositoryBase<TEntity> where TEntity: class 
    {
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> GetAllWithTracking();
        Task<IEnumerable<TEntity>> GetByConditionWithTrackingAsync(Expression<Func<TEntity,bool>> expression);
        Task<IEnumerable<TEntity>> GetByConditionAsync(Expression<Func<TEntity,bool>> expression);
        Task<IEnumerable<TEntity>> GetByConditionWithSorting(Expression<Func<TEntity, bool>> expression, int limit, int offset, string sort, string order);
        Task<TEntity> GetById(int id);
        Task<TEntity> GetByGuid(Guid guid, bool isIncludeLogicalDeletes = false);
    
        Task Create(TEntity entity);
        Task CreateRange(TEntity[] entity);
        void Update(TEntity entity);
        void UpdateRange(TEntity[] entity);
        void Delete(TEntity entity);
        Task SaveAsync();
        Task ExecuteSQL(string sql);
        Task ExecuteSQL(string sql, object[] parameter);
        EntityEntry GetEntry(TEntity entity);
        bool HasUnsavedChanges();
        void LogicalDelete(TEntity entity);
    }
}
