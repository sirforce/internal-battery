﻿using ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UpDiddy.ViewModels.ButterCMS;
using UpDiddyLib.Helpers;

namespace UpDiddy.Services.ButterCMS
{
    public class ButterCMSService : IButterCMSService
    {
        private ICacheService _cacheService = null;
        private IConfiguration _configuration = null;
        private ISysEmail _sysEmail = null;
        private ButterCMSClient _butterClient;

        public ButterCMSService(ICacheService cacheService, IConfiguration configuration, ISysEmail sysEmail)
        {
            _cacheService = cacheService;
            _configuration = configuration;
            _sysEmail = sysEmail;
            _butterClient = new ButterCMSClient(_configuration["ButterCMS:ReadApiToken"]);
        }

        /// <summary>
        /// 
        /// Generic method to leverage the RetrieveContentFields method from ButterCMS
        /// to get BCMS collection using keys and query parameters. The resulting GET
        /// request will look similar to this example:
        /// 
        /// https://api.buttercms.com/v2/content/?keys=careercircle_public_site_navigation&levels=3&auth_token=*AUTH_TOKEN*
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="CacheKey">Key for the Redis cached version of the BCMS response</param>
        /// <param name="Keys">Any keys required to fetch the content</param>
        /// <param name="QueryParameters">Any additional query parameters to modify the GET request</param>
        /// <returns>An object in the form of T representing the BCMS response</returns>
        public T RetrieveContentFields<T>(string CacheKey, string[] Keys, Dictionary<string, string> QueryParameters) where T : class
        {
            T CachedButterResponse = _cacheService.GetCachedValue<T>(CacheKey);
            try
            {
                if (CachedButterResponse == null)
                {
                    CachedButterResponse = _butterClient.RetrieveContentFields<T>(Keys, QueryParameters);
                    
                    if(CachedButterResponse == null)
                    {
                        SendEmailNotification(CacheKey);
                        return null;
                    }
                    _cacheService.SetCachedValue(CacheKey, CachedButterResponse);
                }
            }
            catch(ContentFieldObjectMismatchException Exception)
            {
                SendEmailNotification(CacheKey);
                return null;
            }
            return CachedButterResponse;
        }

        public PageResponse<T> RetrievePage<T>(string CacheKey, string Slug, Dictionary<string, string> QueryParameters) where T : class
        {
            PageResponse<T> CachedButterResponse = _cacheService.GetCachedValue<PageResponse<T>>(CacheKey);
            try
            {
                if (CachedButterResponse == null)
                {
                    CachedButterResponse = _butterClient.RetrievePage<T>("*", Slug, QueryParameters);

                    if (CachedButterResponse == null)
                    {
                        return null;
                    }
                    _cacheService.SetCachedValue(CacheKey, CachedButterResponse);
                }
            }
            catch (ContentFieldObjectMismatchException Exception)
            {
                return null;
            }
            return CachedButterResponse;
            
        }

        public bool ClearCachedValue<T>(string CacheKey)
        {
            return _cacheService.RemoveCachedValue<T>(CacheKey);
        }

        private void SendEmailNotification(string CacheKey)
        {
            /**
             * We're caching that we've sent this email to ensure that as traffic increases,
             * we don't spam the CareerCircle errors inbox upon navigation fetch failure.
             */
            string CacheKeyForNavigationLoadFailure = "HasSentNavigationLoadFailureEmail";
            string HasSentNotificationEmail = _cacheService.GetCachedValue<string>(CacheKeyForNavigationLoadFailure);
            if (string.IsNullOrEmpty(HasSentNotificationEmail))
            {
                StringBuilder HtmlMessage = new StringBuilder();
                HtmlMessage.Append("Error retrieving " + CacheKey + " from ButterCMS, or Redis. Falling back to error navigation.");
                _sysEmail.SendEmailAsync(_configuration["ButterCMS:CareerCirclePublicSiteNavigation:FailedFetchNotifyEmail"],
                    "ALERT! Navigation failed to load.",
                    HtmlMessage.ToString());
                _cacheService.SetCachedValue<string>(CacheKeyForNavigationLoadFailure, "true");
            }
            
        }
    }
}