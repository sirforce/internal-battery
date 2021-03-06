﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddy.Services
{
    public class CacheService : ICacheService
    {
        private IMemoryCache _cache = null;
        private IConfiguration _configuration = null;

        public CacheService(IMemoryCache cache, IConfiguration conifguration)
        {
            _cache = cache;
            _configuration = conifguration;
        }

        public async Task<T> GetCachedValueAsync<T>(string CacheKey)
        {
            try
            {
                string existingValue =  _cache.Get<string>(CacheKey);
                if (string.IsNullOrEmpty(existingValue))
                    return (T)Convert.ChangeType(null, typeof(T));
                else
                {
                    T rval = JsonConvert.DeserializeObject<T>(existingValue);
                    return rval;
                }
            }
            catch (Exception)
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }

        public async Task<bool> SetCachedValueAsync<T>(string CacheKey, T Value)
        {
            int CacheTTL = int.Parse(_configuration["redis:cacheTTLInMinutes"]);
            return await SetCachedValueAsync(CacheKey, Value, DateTimeOffset.Now.AddMinutes(CacheTTL));
        }

        public async Task<bool> SetCachedValueAsync<T>(string CacheKey, T Value, DateTimeOffset expiryTime)
        {
            try
            {
                string newValue = JsonConvert.SerializeObject(Value);
                 _cache.Set<string>(CacheKey, newValue, expiryTime );                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RemoveCachedValueAsync(string CacheKey)
        {
            try
            {                
                 _cache.Remove(CacheKey);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<T> GetSetCachedValueAsync<T>(string CacheKey, Func<Task<T>> func, DateTimeOffset expiryTime)
        {
            T Value = await GetCachedValueAsync<T>(CacheKey);
            if(Value == null)
            {
                Value = await func();
                await SetCachedValueAsync<T>(CacheKey, Value, expiryTime);
            }
            return Value;
        }
    }
}
