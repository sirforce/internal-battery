﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UpDiddyLib.Dto;

namespace UpDiddyLib.Helpers
{
    static public class Utils
    {
        static public string RemoveHTML(string Str)
        {
            return Regex.Replace(Str, "<.*?>", String.Empty);
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
        public static DateTime FromWozTime(long wozTime)
        {
            return epoch.AddMilliseconds(wozTime);
        }

        public static long ToWozTime(DateTime dateTime)
        {
            return (long)(dateTime - epoch).TotalMilliseconds;
        }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static public long CurrentTimeInUnixMilliseconds()
        {
            long rval = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return rval;
        }

        static public long CurrentTimeInUnixSeconds()
        {
            long rval = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return rval;
        }


        static public DateTime PriorDayOfWeek(DateTime StartTime, System.DayOfWeek DayOfTheWeek)
        {           
            int DaysApart = StartTime.DayOfWeek - DayOfTheWeek;
            if (DaysApart < 0) DaysApart += 7;
            DateTime PriorDay = StartTime.AddDays(-1 * DaysApart);

            return PriorDay;
        }
    }
}
