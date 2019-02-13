﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyLib.Helpers
{
    // todo: consider moving these constants to their respective domains of usage
    public static class Constants
    {
        public const string TRACKING_KEY_CONTACT = "Contact";
        public const string TRACKING_KEY_ACTION = "Action";
        public const string TRACKING_KEY_CAMPAIGN = "Campaign";


        static public string  SubsriberSessionKey = "Subscriber";
        static public readonly string EMPTY_STRING = "";
        static public readonly string HttpGetClientName = "HttpGetClient";
        static public readonly string HttpPostClientName = "HttpPostClient";
        static public readonly string HttpPutClientName = "HttpPutClient";
        static public readonly string HttpDeleteClientName = "HttpDeleteClient";
        static public readonly string PollyStringCacheName = "PollyStringCacheName";
        static public readonly int PollyStringCacheTimeInMinutes = 5;
        static public readonly string SysLogLogInformationTrue = "true";
        static public readonly string WozVendorName = "WozU";
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
        }

        public static class SignalR
        {
            public static readonly string CookieKey = "ccsignalr_connection_id";
            public static readonly string ResumeUpLoadVerb = "UploadResume";
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
    }
}
