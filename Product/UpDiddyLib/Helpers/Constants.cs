﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyLib.Helpers
{
    // todo: consider moving these constants to their respective domains of usage
    public static class Constants
    {
        public const string TRACKING_KEY_PARTNER_CONTACT = "Contact";
        public const string TRACKING_KEY_ACTION = "Action";
        public const string TRACKING_KEY_CAMPAIGN = "Campaign";
        public const string TRACKING_KEY_CAMPAIGN_PHASE = "CampaignPhase";
        public const string TRACKING_KEY_JOB_APPLICATION = "JobApplication";
        public const string TRACKING_KEY_TINY_ID = "TinyId";

        static public string SubsriberSessionKey = "Subscriber";
        static public readonly string EMPTY_STRING = "";
        static public readonly string HttpGetClientName = "HttpGetClient";
        static public readonly string HttpPostClientName = "HttpPostClient";
        static public readonly string HttpPutClientName = "HttpPutClient";
        static public readonly string HttpDeleteClientName = "HttpDeleteClient";
        static public readonly string PollyStringCacheName = "PollyStringCacheName";
        static public readonly int PollyStringCacheTimeInMinutes = 5;
        static public readonly string SysLogLogInformationTrue = "true";
        static public readonly string WozVendorName = "WozU";
        static public readonly Guid WozVendorGuid = new Guid("00000000-0000-0000-0000-000000000001");
        static public readonly string RegionCodeUS = "US";
        static public readonly List<String> ValidTextFileExtensions = new List<String>
        {
            "doc", "docx", "odt", "pdf", "rtf", "tex", "txt", "wks", "wps", "wpd"
        };
        static public readonly string NotSpecifedOption = "Not Specified";

        public static class DataFormat
        {
            public static readonly string Json = "Json";
            public static readonly string Xml = "Xml";
        }

        // data sources for subscriber staging store
        public static class DataSource
        {
            public static readonly string Sovren = "Sovren";
            public static readonly string LinkedIn = "LinkedIn";
            public static readonly string CareerCircle = "CareerCircle";
        }

        public static class SignalR
        {
            public static readonly string CookieKey = "ccsignalr_connection_id";
            public static readonly string ResumeUpLoadVerb = "UploadResume";
            public static readonly string ResumeUpLoadAndParseVerb = "ResumeUpLoadAndParseVerb";
        }

        public static class CampaignRebate
        {
            public static readonly string CourseCompletion_Completed_EligibleMsg = "Congratulations on your new skill! Contact customer support to process your rebate";
            public static readonly string CourseCompletion_Completed_NotEligibleMsg = "Congratulations on your new skill! Contact customer support for job oppurtunities";
            public static readonly string CourseCompletion_InProgress_EligibleMsg = "Complete in {0} days for a full rebate";
            public static readonly string CourseCompletion_InProgress_NotEligibleMsg = "Rebate offer expired";
            public static readonly string Employment_Completed_EligibleMsg = "Congratulations on your new skill! Contact customer support for a full rebate if hired by one of our partners within {0} days";
            public static readonly string Employment_InProgress_EligibleMsg = "This course is free when you get hired by one of our partners within {0} days";
            public static class CampaignRebateType
            {
                public static readonly string Employment = "Employment";
                public static readonly string CourseCompletion = "Course completion";
            }

        }

        public static class JobPosting
        {
            public static readonly string ValidationError_CompanyRequiredMsg = "Company required";
            public static readonly string ValidationError_InvalidSecurityClearanceMsg = "Invalid security clearance";
            public static readonly string ValidationError_InvalidIndustryMsg = "Invalid industry";
            public static readonly string ValidationError_InvalidJobCategoryMsg = "Invalid job category";
            public static readonly string ValidationError_InvalidEducationLevelMsg = "Invalid education level";
            public static readonly string ValidationError_InvalidExperienceLevelMsg = "Invalid experience level";
            public static readonly string ValidationError_InvalidEmploymentTypeMsg = "Invalid employment type";
            public static readonly string ValidationError_SubscriberRequiredMsg = "Subscriber is required";
            public static readonly string ValidationError_InvalidDescriptionLength = "Posting Description must contain at least {0} characters";
            public static readonly string ValidationError_JobNotIndexed = "Job has not been indexed";
        }

        public enum SendGridAccount
        {
            Transactional,
            NotifySystem
        }

        public static class CMS
        {
            public static readonly string CACHE_KEY_PREFIX = "_cms_";
            public static readonly string PAGE_CACHE_KEY_PREFIX = "page";
            public static readonly string NULL_RESPONSE = "NULL_RESPONSE";
            public static readonly string RESPONSE_RECEIVED = "RESPONSE_RECEIVED";
            public static readonly string LEVELS = "levels";
            public static readonly int BLOG_PAGINATION_PAGE_COUNT = 10;
            public static readonly string COURSE_CACHE_KEY_PREFIX = "course";
            public static readonly string BLOG_TITLE_TAG_SUFFIX = " | CareerCircle Blog";
        }

        public static class Seo
        {
            public static readonly string TITLE = "title";
            public static readonly string META_DESCRIPTION = "meta_description";
            public static readonly string META_KEYWORDS = "meta_keywords";

            public static readonly string OG_TITLE = "og_title";
            public static readonly string OG_DESCRIPTION = "og_description";
            public static readonly string OG_IMAGE = "og_image";
        }

        public static class CrossReference
        {
            public static class Group
            {
                public static readonly string WOZ_STUDENT = "Woz Student";
            }
        }

        public static class EventType
        {
            public static readonly string JobPosting = "Job posting";
            public static readonly string FileDownloadTracker = "File Download Tracker";
            public static readonly string TraitifyAssessment = "Traitify Assessment";

        }

        public static class Action
        {
            public static readonly string ApplyJob = "Apply job";
            public static readonly string View = "View";
            public static readonly string DownloadGatedFile = "Download Gated File";
            public static readonly string TraitifyAccountCreation = "Traitify Account Creation";
        }


        public static class HiringSolvedStatus
        {
            public static readonly string Created = "created";
            public static readonly string Queued = "queued";
            public static readonly string Finished = "finished";
            public static readonly string Failed = "failed";
        }

        public static class AzureSearchIndexStatus
        {
            public static readonly string None = "None";
            public static readonly string Pending = "Pending";
            public static readonly string Indexed = "Indexed";
            public static readonly string Deleted = "Deleted";
            public static readonly string Error = "Error";
        }


 
    }
}
