﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UpDiddyLib.Dto;
using UpDiddy.Helpers;

namespace UpDiddyLib.Helpers
{




    static public class Utils
    {
      
        public static string ToBase64EncodedString(IFormFile file)
        {
            string base64EncodedString = null;

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                var fileBytes = stream.ToArray();
                base64EncodedString = Convert.ToBase64String(fileBytes);
            }

            return base64EncodedString;
        }

        static public List<string> ParseSkillsFromHrXML(string xml)
        {
            List<string> rVal = new List<String>();
 
                XElement theXML = XElement.Parse(xml);
                // Get list of skill found by Sovren
                var skills = theXML.Descendants()
                     .Where(e => e.Name.LocalName == "Skill")
                     .ToList();
                // Iterate over their skills 
                foreach (XElement node in skills)
                    rVal.Add(node.Attribute("name").Value.Trim());

            return rVal;

        }

        public static Boolean IsValidTextFile(string Filename)
        {
            // Ensure the filename string is valid
            if (Filename == null || string.IsNullOrEmpty(Filename) || !Filename.Contains('.'))
            {
                return false;
            }

            string[] splitFileName = Filename.Split(".");
            return Constants.ValidTextFileExtensions.Contains(splitFileName[splitFileName.Length - 1]);

        }

        static public string RemoveHTML(string Str)
        {
            return Regex.Replace(Str, "<.*?>", String.Empty);
        }

        static public string RemoveQueryStringFromUrl(string url)
        {
            int idx = url.IndexOf("?");
            if (idx == -1)
                return url;
            else
                return url.Substring(0, idx);
        }


        static public T JTokenConvert<T>(JToken o, T defaultValue) 
        {
            try
            {
                if (o == null)
                    return defaultValue;
                return (T)Convert.ChangeType(o.ToString(), typeof(T));          
            }
            catch
            {
                return defaultValue;
            } 

        }


        static public string RemoveNewlines(string Str)
        {
            return Regex.Replace(Str, "\r\n", String.Empty);
        }

        static public string RemoveRedundantSpaces(string Str)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(Str.Trim(), " ");

        }
        public static DateTime FromUnixTimeInMilliseconds(long wozTime)
        {
            return epoch.AddMilliseconds(wozTime);
        }

        public static long ToUnixTimeInMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime - epoch).TotalMilliseconds;
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static public DateTime PriorDayOfWeek(DateTime StartTime, System.DayOfWeek DayOfTheWeek)
        {           
            int DaysApart = StartTime.DayOfWeek - DayOfTheWeek;
            if (DaysApart < 0) DaysApart += 7;
            DateTime PriorDay = StartTime.AddDays(-1 * DaysApart);

            return PriorDay;
        }
    }
}
