﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
 using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Factory
{

    public enum ProfileDataStatus { Acquired = 0, Processing, Processed, Deleted, AccountNotFound, AcquistionError, ProcessingError };

    public class SubscriberProfileStagingStoreFactory
    {
        public static SubscriberProfileStagingStore CreateSubscriberProfileStagingStore(UpDiddyDbContext db, Guid subscriberGuid)
        {
            // todo: see if dotnet has built in behaviors or checks for required fields such as subscriber guid
            var Subscriber = db.Subscriber
            .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
              .FirstOrDefault();

            if (Subscriber == null)
                throw new Exception($"LinkedInInterface:ImportProfile Subscriber not found for {subscriberGuid}");

            SubscriberProfileStagingStore rVal = new SubscriberProfileStagingStore();
            rVal.CreateDate = DateTime.UtcNow;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.ModifyGuid = Guid.Empty;
            rVal.CreateGuid = Guid.Empty;
            rVal.SubscriberId = Subscriber.SubscriberId;
            rVal.ProfileSource = Constants.DataSource.LinkedIn;
            rVal.IsDeleted = 0;
            rVal.ProfileFormat = Constants.DataFormat.Json;

            return rVal;
        }

        // TODO possibly modularize after knowing the complete set of linkedin data that will be available 
        // with the access keys from the lab 
        public static LinkedInProfileDto ToLinkedInDto(SubscriberProfileStagingStore pss, ILogger sysLog)
        {
            LinkedInProfileDto rVal = null;

            try
            {
                JObject o = JObject.Parse(pss.ProfileData);
                rVal = new LinkedInProfileDto();
                // Parse root level items 
                rVal.Id = Utils.JTokenConvert<string>(o.SelectToken("$.id"), string.Empty);
                rVal.FirstName = Utils.JTokenConvert<string>(o.SelectToken("$.firstName"), string.Empty);
                rVal.LastName = Utils.JTokenConvert<string>(o.SelectToken("$.lastName"), string.Empty);
                rVal.MaidenName = Utils.JTokenConvert<string>(o.SelectToken("$.maidenName"), string.Empty);
                rVal.FormattedName = Utils.JTokenConvert<string>(o.SelectToken("$.formattedName"), string.Empty);
                rVal.PhoneticFirstName = Utils.JTokenConvert<string>(o.SelectToken("$.phoneticFirstName"), string.Empty);
                rVal.PhoneticLastName = Utils.JTokenConvert<string>(o.SelectToken("$.phoneticLastName"), string.Empty);
                rVal.FormattedPhoneticName = Utils.JTokenConvert<string>(o.SelectToken("$.formattedPhoneticName"), string.Empty);
                rVal.CurrentShare = Utils.JTokenConvert<string>(o.SelectToken("$.currentShare"), string.Empty);
                rVal.Summary = Utils.JTokenConvert<string>(o.SelectToken("$.summary"), string.Empty);
                rVal.Industry = Utils.JTokenConvert<string>(o.SelectToken("$.industry"), string.Empty);
                rVal.NumConections = Utils.JTokenConvert<int>(o.SelectToken("$.numConections"), -1);
                rVal.NumConnectionsCapped = Utils.JTokenConvert<int>(o.SelectToken("$.numConnectionsCapped"), -1);
                rVal.PictureUrl = Utils.JTokenConvert<string>(o.SelectToken("$.pictureUrl"), string.Empty);
                rVal.SiteStandardProfileRequest = Utils.JTokenConvert<string>(o.SelectToken("$.siteStandardProfileRequest"), string.Empty);
                rVal.ApiStandardProfileRequest = Utils.JTokenConvert<string>(o.SelectToken("$.apiStandardProfileRequest"), string.Empty);
                rVal.PublicProfileUrl = Utils.JTokenConvert<string>(o.SelectToken("$.publicProfileUrl"), string.Empty);
                rVal.Specialties = Utils.JTokenConvert<string>(o.SelectToken("$.specialties"), string.Empty);

                // Parse positions 
                IEnumerable<JToken> positions = o.SelectTokens("$.positions.values[*]");

                if (positions.Count<JToken>() > 0)
                {
                    rVal.Positions = new List<LinkedInPositionDto>();
                    foreach (JToken item in positions)
                    {
                        LinkedInPositionDto pos = new LinkedInPositionDto();
                        pos.CompanyName = Utils.JTokenConvert<string>(item.SelectToken("$..company.name"), string.Empty);
                        pos.Id = Utils.JTokenConvert<int>(item.SelectToken("$.id"), -1);
                        pos.Title = Utils.JTokenConvert<string>(item.SelectToken("$.title"), string.Empty);
                        pos.Summary = Utils.JTokenConvert<string>(item.SelectToken("$.summary"), string.Empty);
                        pos.IsCurrent = Utils.JTokenConvert<bool>(item.SelectToken("$.isCurrent"), false);
                        pos.StartDate = _ConvertLinkedInDate(item.SelectToken("$.startDate"));
                        pos.EndDate = Utils.JTokenConvert<DateTime>(item.SelectToken("$.endDate"), DateTime.MinValue);
                        rVal.Positions.Add(pos);
                    };

                }
                // Parse locations 
                rVal.LocationCountry = Utils.JTokenConvert<string>(o.SelectToken("$..location.countryCode"), string.Empty);
                rVal.Location = Utils.JTokenConvert<string>(o.SelectToken("$..location.name"), string.Empty);
            }
            catch (Exception e)
            {
                sysLog.LogError("SubscriberProfileStagingStore.ToLinkedInDto threw and exception -> " + e.Message, pss);
            }
            return rVal;
        }

        #region Factory Methods 

        public static LinkedInProfileDto GetProfileAsLinkedInDto(UpDiddyDbContext db, Guid subscriberGuid, ILogger sysLog)
        {
            SubscriberProfileStagingStore pss = GetBySubcriber(db, subscriberGuid);
            if (pss == null)
                return null;
            else
                return ToLinkedInDto(pss, sysLog);
        }


        public static SubscriberProfileStagingStore GetBySubcriber(UpDiddyDbContext db, Guid subscriberGuid)
        {
            SubscriberProfileStagingStore pss = db.SubscriberProfileStagingStore
                 .Include(s => s.Subscriber)
                 .Where(s => s.IsDeleted == 0 && s.Subscriber.SubscriberGuid == subscriberGuid && s.ProfileSource == Constants.DataSource.LinkedIn)
                 .OrderByDescending(s => s.ModifyDate)
             .FirstOrDefault();

            return pss;
        }


        public static void StoreProfileData(UpDiddyDbContext db, Guid subscriberGuid, string ResponseJson)
        {
            SubscriberProfileStagingStore pss = CreateSubscriberProfileStagingStore(db, subscriberGuid);

            pss.ModifyDate = DateTime.UtcNow;
            pss.ProfileData = ResponseJson;
            pss.Status = (int)ProfileDataStatus.Acquired;

            db.SubscriberProfileStagingStore.Add(pss);
            db.SaveChanges();
        }

        // todo: maybe we have this interact with interfaces so that it knows how to grab the data, srcName, format, data
        public static void Save(UpDiddyDbContext db, Subscriber subscriber, string srcName, string format, string data)
        {
            SubscriberProfileStagingStore stagingStore = new SubscriberProfileStagingStore();
            stagingStore.CreateDate = DateTime.UtcNow;
            stagingStore.ModifyDate = DateTime.UtcNow;
            stagingStore.ModifyGuid = Guid.Empty;
            stagingStore.ModifyGuid = Guid.Empty;
            stagingStore.SubscriberId = subscriber.SubscriberId;
            stagingStore.ProfileSource = srcName;
            stagingStore.ProfileFormat = format;
            stagingStore.ProfileData = data;
            stagingStore.Status = (int)ProfileDataStatus.Acquired;

            db.SubscriberProfileStagingStore.Add(stagingStore);
            db.SaveChanges();
        }

        public static async Task Save(IRepositoryWrapper repositoryWrapper, Subscriber subscriber, string srcName, string format, string data)
        {
            SubscriberProfileStagingStore stagingStore = new SubscriberProfileStagingStore();
            stagingStore.CreateDate = DateTime.UtcNow;
            stagingStore.ModifyDate = DateTime.UtcNow;
            stagingStore.ModifyGuid = Guid.Empty;
            stagingStore.ModifyGuid = Guid.Empty;
            stagingStore.SubscriberId = subscriber.SubscriberId;
            stagingStore.ProfileSource = srcName;
            stagingStore.ProfileFormat = format;
            stagingStore.ProfileData = data;
            stagingStore.Status = (int)ProfileDataStatus.Acquired;

            await repositoryWrapper.SubscriberProfileStagingStoreRepository.Create(stagingStore);
            await repositoryWrapper.SaveAsync();
        }


        #endregion


        #region Helper methods 

        private static DateTime? _ConvertLinkedInDate(JToken o)
        {
            try
            {
                if (o == null)
                    return null;

                string month = Utils.JTokenConvert<string>(o.SelectToken("$.month"), string.Empty);
                string year = Utils.JTokenConvert<string>(o.SelectToken("$.year"), string.Empty);

                DateTime? rVal = null;
                if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(month))
                    rVal = (DateTime?)new DateTime(int.Parse(year), int.Parse(month), 1);

                return rVal;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }

}
