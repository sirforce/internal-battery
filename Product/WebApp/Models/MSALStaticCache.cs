﻿using Microsoft.Identity.Client;

using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace UpDiddy.Models
{
    public class MSALStaticCache
    {
        private static Dictionary<string, byte[]> staticCache = new Dictionary<string, byte[]>();

        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string UserId = string.Empty;
        string CacheId = string.Empty;
        HttpContext httpContext = null;

        ITokenCache cache;

        public MSALStaticCache(string userId, HttpContext httpcontext)
        {
            // not object, we want the SUB
            UserId = userId;
            CacheId = UserId + "_TokenCache";
            httpContext = httpcontext;
        }

        public ITokenCache EnablePersistence(ITokenCache cache)
        {
            this.cache = cache;
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
 
          //  Load();
            return cache;
        }

        public void Load(ITokenCacheSerializer  tokenCacheSerializer)
        {
            SessionLock.EnterReadLock();
            byte[] blob = staticCache.ContainsKey(CacheId) ? staticCache[CacheId] : null;
            if (blob != null)
            {
                tokenCacheSerializer.DeserializeMsalV3(blob);
            }
            SessionLock.ExitReadLock();
        }

        public void Persist(ITokenCacheSerializer tokenCacheSerializer)
        {
            SessionLock.EnterWriteLock();

            // Reflect changes in the persistent store
            staticCache[CacheId] = tokenCacheSerializer.SerializeMsalV3();
            SessionLock.ExitWriteLock();
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load(args.TokenCache);
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                
                Persist(args.TokenCache);
            }
        }
 


    }
}