﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UpDiddyLib.Dto;
using System.Xml;
using System.Globalization;
using System.Reflection;
using GoogleTypes = Google.Protobuf.WellKnownTypes;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Net.Mail;

namespace UpDiddyLib.Helpers
{
    static public class Utils
    {
        /// <summary>
        /// Ensures that a new password conforms to our ADB2C complexity requirements:
        /// Minimum 8 characters and maximum 16 characters in length 3 of 4 character classes - uppercase, lowercase, number, symbol. 
        /// A static error message is used for password validation. This setting is legacy behavior. We recommend strong instead.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool IsPasswordPassesADB2CRequirements(string password)
        {
            Regex legacyAdb2cPasswordComplexity = new Regex(@"(?=.{8,})((?=.*\d)(?=.*[a-z])(?=.*[A-Z])|(?=.*\d)(?=.*[a-zA-Z])(?=.*[\W_])|(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_])).*");
            return legacyAdb2cPasswordComplexity.IsMatch(password);
        }

        /// <summary>
        /// Ensures that a new password conforms to our Auth0 complexity requirements:
        /// - No more than 2 identical characters in a row
        /// - Special characters(!@#$%^&*)
        /// - Lower case (a-z), upper case (A-Z) and numbers(0-9)
        /// - Must have 1 characters in length
        /// - Non-empty password required
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool IsPasswordPassesAuth0Requirements(string password)
        {
            Regex atLeastOneSpecialCharacter = new Regex(@"[!@#\$%\^&\*]{1}");
            Regex noMoreThanTwoConsecutiveCharacters = new Regex(@"^((.)\2?(?!\2))+$");
            Regex lowerCaseUpperCaseAndNumbers = new Regex(@"(?=.*\d)(?=.*[a-z])(?=.*[A-Z])");

            return !string.IsNullOrWhiteSpace(password) &&
                password.Length > 1 &&
                atLeastOneSpecialCharacter.IsMatch(password) &&
                noMoreThanTwoConsecutiveCharacters.IsMatch(password) &&
                lowerCaseUpperCaseAndNumbers.IsMatch(password);
        }

        public static int GetIntQueryParam(IQueryCollection queryInfo, string ParamName, int defaultValue = 0)
        {
            int rVal = defaultValue;
            // first check to see if the param was specified in the query string.  Highest priority 
            if (queryInfo.Keys.Contains(ParamName) && string.IsNullOrEmpty(queryInfo[ParamName]) == false && queryInfo[ParamName] != "all")
                int.TryParse(WebUtility.UrlDecode(queryInfo[ParamName]).Trim(), out rVal);

            return rVal;
        }



        public static string GetQueryParam(IQueryCollection queryInfo, string ParamName, string defaultValue = "")
        {
            string rVal = defaultValue;
            // first check to see if the param was specified in the query string.  Highest priority 
            if (queryInfo.Keys.Contains(ParamName) && string.IsNullOrEmpty(queryInfo[ParamName]) == false && queryInfo[ParamName] != "all")
                return WebUtility.UrlDecode(queryInfo[ParamName]).Trim();

            return defaultValue;
        }


        public static string QueryParamsToCacheKey(string keyBase,  IQueryCollection query)
        {
            var sortedQuery = query.OrderBy(q => q.Key);
            string cacheKey = string.Join("", sortedQuery.Select(q => q.Key + q.Value));
            return keyBase + cacheKey;
        }

        public static string  AlphaNumeric(string input, int maxLen)
        {

            if (input == null)
                return string.Empty;

            Regex rgx = new Regex("[^a-zA-Z0-9]");
            string rVal  = rgx.Replace(input, "");

            if (rVal.Length > maxLen)
                rVal = rVal.Substring(0, (maxLen - 1) );

            return rVal;
        }

 
        public static string GeneratePassword(bool requireNonAlphaNumeric, bool requireDigit, bool requireLowercase, bool requireUppercase, int requiredLength)
        {
            StringBuilder password = new StringBuilder();
            Random random = new Random();

            while (password.Length < requiredLength)
            {
                char c = (char)random.Next(32, 126);

                password.Append(c);

                if (char.IsDigit(c))
                    requireDigit = false;
                else if (char.IsLower(c))
                    requireLowercase = false;
                else if (char.IsUpper(c))
                    requireUppercase = false;
                else if (!char.IsLetterOrDigit(c))
                    requireNonAlphaNumeric = false;
            }

            if (requireNonAlphaNumeric)
                password.Append((char)random.Next(33, 48));
            if (requireDigit)
                password.Append((char)random.Next(48, 58));
            if (requireLowercase)
                password.Append((char)random.Next(97, 123));
            if (requireUppercase)
                password.Append((char)random.Next(65, 91));

            return password.ToString();
        }

        public static string EscapeQuoteEmailsInString(string keywords)
        {
            var a =
@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])";

            var s = Regex.Replace(keywords, a, m =>
            {
                //Console.WriteLine(m.Groups[0].Value);
                return @"""" + m.Groups[0].Value + @"""";
            }
            , RegexOptions.IgnoreCase);

            return s;
        }


        // quick and dirty email validation class 
        public static bool ValidateEmail(string emailaddress)
        {
            return Regex.IsMatch(emailaddress, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }



        public static bool validStartDate(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || startDate.Value != DateTime.MinValue)
                return false;

            if (endDate != null && endDate.Value != DateTime.MinValue && startDate.Value >= endDate.Value)
                return false;

            return true;
        }

        public static bool validEndDate(DateTime? startDate, DateTime? endDate)
        {
            if (endDate == null || endDate.Value != DateTime.MinValue)
                return false;

            if (startDate != null && startDate.Value != DateTime.MinValue && endDate.Value < startDate.Value)
                return false;

            return true;
        }


        public static DateTime Next(this DateTime from, DayOfWeek dayOfWeek)
        {
            int start = (int)from.DayOfWeek;
            int wanted = (int)dayOfWeek;
            if (wanted <= start)
                wanted += 7;
            return from.AddDays(wanted - start);
        }

        public static DateTime Previous(this DateTime from, DayOfWeek dayOfWeek)
        {
            int end = (int)from.DayOfWeek;
            int wanted = (int)dayOfWeek;
            if (wanted >= end)
                end += 7;
            return from.AddDays(wanted - end);
        }

        public static bool GetImageAsBlob(string imgUrl, int maxSize, ref byte[] imageBytes)
        {

            HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(imgUrl);
            WebResponse imageResponse = imageRequest.GetResponse();
            Stream responseStream = imageResponse.GetResponseStream();

            bool rVal = true;
            using (BinaryReader br = new BinaryReader(responseStream))
            {
                imageBytes = br.ReadBytes(maxSize);
                // check for eos
                if (br.PeekChar() != -1)
                    rVal = false;
                br.Close();
            }
            responseStream.Close();
            imageResponse.Close();

            return rVal;
        }

        /// <summary>
        /// Returns the path of a semantic job url. Note that this does not include other components of the url (protocol, scheme, host, query string)
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="industry"></param>
        /// <param name="jobCategory"></param>
        /// <param name="country"></param>
        /// <param name="province"></param>
        /// <param name="city"></param>
        /// <param name="jobIdentifier"></param>
        /// <returns></returns>
        public static string CreateSemanticJobPath(string industry, string jobCategory, string country, string province, string city, string jobIdentifier)
        {
            StringBuilder jobPath = new StringBuilder("/job/");

            // industry 
            if (!string.IsNullOrWhiteSpace(industry))
                jobPath.Append(industry);
            else
                jobPath.Append("all");
            // job category 
            jobPath.Append("/");
            if (!string.IsNullOrWhiteSpace(jobCategory))
                jobPath.Append(jobCategory);
            else
                jobPath.Append("all");
            //  country  
            jobPath.Append("/");
            if (!string.IsNullOrWhiteSpace(country))
                jobPath.Append(country);
            else
                jobPath.Append("all");
            // state 
            jobPath.Append("/");
            if (!string.IsNullOrWhiteSpace(province))
                jobPath.Append(province);
            else
                jobPath.Append("all");
            // city 
            jobPath.Append("/");
            if (!string.IsNullOrWhiteSpace(city))
                jobPath.Append(city);
            else
                jobPath.Append("all");
            // job identifier 
            jobPath.Append("/");
            jobPath.Append(jobIdentifier);

            return jobPath.ToString().ToUrlSlug();
        }

        public static string ToUrlSlug(this string value)
        {
            //First to lower case
            value = value.ToLowerInvariant();
            //Remove all accents - removing this code for now; it worked once but having trouble getting the Cyrillic encoding to be recognized even after including System.Text.Encoding.CodePages
            //var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(value);
            //value = Encoding.ASCII.GetString(bytes);
            //Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);
            //Remove invalid chars
            value = Regex.Replace(value, @"[^a-z0-9\s-_\/]", "", RegexOptions.Compiled);
            //Trim dashes from end
            value = value.Trim('-', '_');
            //Replace double occurences of - or _
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);
            return value;
        }


        /// <summary>
        /// Convert a datetime object to a ISO8601 date string 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ISO8601DateString(DateTime dt)
        {
            return dt.ToString("o");
        }
        /// <summary>
        /// Return the specified datetime as a google timestamp string 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string GetTimestampAsString(DateTime dt)
        {
            var NumSeconds = new DateTimeOffset(dt).ToUnixTimeSeconds();
            GoogleTypes.Timestamp ts = new GoogleTypes.Timestamp();
            ts.Seconds = NumSeconds;
            return TimeStampToISO8601String(ts);
        }

        /// <summary>
        /// Convert datetime to google timestampe
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static GoogleTypes.Timestamp GetTimestamp(DateTime dt)
        {
            var NumSeconds = new DateTimeOffset(dt).ToUnixTimeSeconds();
            GoogleTypes.Timestamp ts = new GoogleTypes.Timestamp();
            ts.Seconds = NumSeconds;
            return ts;
        }

        /// <summary>
        /// Convert google timestampe to a string.  For some reason the Timestamp.ToString() function
        /// returns a string with enclosed in escaped double quotes such as "\"2020-10-02T15:01:23.045123456Z\""
        /// not "2020-10-02T15:01:23.045123456Z" which this function returns
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string TimeStampToISO8601String(GoogleTypes.Timestamp ts)
        {
            DateTime tsDateTime = ts.ToDateTime();
            return tsDateTime.ToString("o");
        }

        /// <summary>
        /// For a provided email address, returns an unrecognizable representation of that email address to anyone that
        /// doesn't already know what it is (e.g. johnsmith@mail.com => j*******h@m**l.com). If an invalid email is provided,
        /// an empty string is returned.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static string ObfuscateEmail(string email)
        {
            StringBuilder obfuscatedEmail = new StringBuilder();

            try
            {
                int lastIndexOfPeriod = email.LastIndexOf('.');
                int indexOfAt = email.IndexOf('@');

                string localPart = email.Substring(0, indexOfAt);
                string topLevelDomain = email.Substring(lastIndexOfPeriod + 1, email.Length - (lastIndexOfPeriod + 1));
                string secondLevelDomain = email.Substring(indexOfAt + 1, (lastIndexOfPeriod - indexOfAt) - 1);

                obfuscatedEmail.Append(localPart.Substring(0, 1));
                obfuscatedEmail.Append(new String('*', localPart.Length - 2));
                obfuscatedEmail.Append(localPart.Substring(localPart.Length - 1, 1));
                obfuscatedEmail.Append("@");
                obfuscatedEmail.Append(secondLevelDomain.Substring(0, 1));
                obfuscatedEmail.Append(new String('*', secondLevelDomain.Length - 2));
                obfuscatedEmail.Append(secondLevelDomain.Substring(secondLevelDomain.Length - 1, 1));
                obfuscatedEmail.Append(".");
                obfuscatedEmail.Append(topLevelDomain);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return obfuscatedEmail.ToString();
        }

        /// <remarks>
        /// Shamelessly stolen from https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
        /// </remarks>
        /// <summary>
        /// Checks to see if a type is derived from a generic type
        /// </summary>
        /// <param name="generic"></param>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.GetTypeInfo().BaseType;
            }
            return false;
        }

        /// <summary>
        /// Convert muli-words search queries into a sql full text search query e.g. "gmail com" -> "gmail* AND com"
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToSqlServerFullTextQuery(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            s = Regex.Replace(s, @"[(,)'""]", string.Empty).Trim();
            s = Regex.Replace(s, @"(\b )", @"* AND ");
            s = Regex.Replace(s, @"(\b$)", @"*");

            return s;
        }

        /// <summary>
        /// Generic formatting method to provide a placeholder when no value exists
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string FormatGenericField(string fieldValue, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                return $"No {fieldName} provided";
            }
            else
            {
                return fieldValue;
            }
        }

        /// <summary>
        /// Formats a date range depending upon the values provided for start and end date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string FormatDateRange(DateTime? startDate, DateTime? endDate, string fieldName)
        {
            string periodDisplay = string.Empty;
            if (!startDate.HasValue)
            {
                periodDisplay = $"No {fieldName} date range specified";
            }
            else
            {
                DateTime effectiveEndDate;
                periodDisplay = startDate.Value.ToString("MMMM yyyy") + " - ";
                if (!endDate.HasValue || endDate.Value == DateTime.MinValue)
                {
                    effectiveEndDate = DateTime.UtcNow;
                    periodDisplay += "Present";
                }
                else
                {
                    effectiveEndDate = endDate.Value;
                    periodDisplay += endDate.Value.ToString("MMMM yyyy");
                }

                if (endDate > startDate)
                {
                    var period = new DateTime(effectiveEndDate.Subtract(startDate.Value).Ticks);
                    periodDisplay += " (" + period.Year + " years " + period.Month + " months)";
                }
            }
            return periodDisplay;
        }

        /// <summary>
        /// Formats an educational degree and type depending upon which fields have values
        /// </summary>
        /// <param name="degree"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FormatEducationalDegreeAndType(string degree, string type)
        {
            string degreeDisplay = degree;
            if (!string.IsNullOrWhiteSpace(type))
            {
                degreeDisplay += $" ({type})";
            }
            if (string.IsNullOrWhiteSpace(degreeDisplay))
            {
                degreeDisplay = "No degree specified";
            }
            return degreeDisplay;
        }

        /// <summary>
        /// Formats a city, state, and postal code with commas where appropriate depending upon which fields have values
        /// </summary>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="postalCode"></param>
        /// <returns></returns>
        public static string FormatCityStateAndPostalCode(string city, string state, string postalCode)
        {
            if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(state) && string.IsNullOrWhiteSpace(postalCode))
            {
                return "No city, state, or postal code provided";
            }
            else if (!string.IsNullOrWhiteSpace(city))
            {
                if (!string.IsNullOrWhiteSpace(state) || !string.IsNullOrWhiteSpace(postalCode))
                {
                    return $"{city}, {state} {postalCode}".Trim();
                }
                else
                {
                    return city;
                }
            }
            else
            {
                return $"{state} {postalCode}".Trim();
            }
        }

        /// <summary>
        /// Formats a name differently depending upon which name fields have values
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public static string FormatName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            {
                return "No name provided";
            }
            else if (string.IsNullOrWhiteSpace(firstName))
            {
                return lastName;
            }
            else if (string.IsNullOrWhiteSpace(lastName))
            {
                return firstName;
            }
            else
            {
                return $"{lastName}, {firstName}";
            }
        }

        public static string FormatPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return "No phone number provided";
            if (phoneNumber.Length < 10 || phoneNumber.Length > 15 || !Regex.Match(phoneNumber, @"^[0-9]+$").Success)
                return phoneNumber;
            int max = 15, min = 10;
            string areaCode = phoneNumber.Substring(0, 3);
            string mid = phoneNumber.Substring(3, 3);
            string lastFour = phoneNumber.Substring(6, 4);
            string extension = phoneNumber.Substring(10, phoneNumber.Length - min);
            if (phoneNumber.Length == min)
            {
                return $"({areaCode}) {mid}-{lastFour}";
            }
            else if (phoneNumber.Length > min && phoneNumber.Length <= max)
            {
                return $"({areaCode}) {mid}-{lastFour} x{extension}";
            }
            return phoneNumber;
        }

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

        static public SubscriberContactInfoDto ParseContactInfoFromHrXML(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string defaultXlms = doc.DocumentElement.NamespaceURI;
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("hrxml", defaultXlms);

            string firstName = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:PersonName/hrxml:GivenName", namespaceManager));
            string lastName = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:PersonName/hrxml:FamilyName", namespaceManager));
            string email = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:InternetEmailAddress", namespaceManager));
            string phoneNumber = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:Telephone/hrxml:FormattedNumber", namespaceManager));
            string address = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:DeliveryAddress/hrxml:AddressLine", namespaceManager));
            string state = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:Region", namespaceManager));
            string countryCode = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:Country", namespaceManager));
            string city = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:Municipality", namespaceManager));
            string postalCode = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:StructuredXMLResume/hrxml:ContactInfo/hrxml:ContactMethod/hrxml:PostalAddress/hrxml:PostalCode", namespaceManager));

            SubscriberContactInfoDto rVal = new SubscriberContactInfoDto()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                Address = address,
                State = state,
                CountryCode = countryCode,
                City = city,
                PostalCode = postalCode
            };

            return rVal;
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


        static public List<SubscriberEducationHistoryDto> ParseEducationHistoryFromHrXml(string xml)
        {
            List<SubscriberEducationHistoryDto> rVal = new List<SubscriberEducationHistoryDto>();

            XElement theXML = XElement.Parse(xml);
            // Get list of skill found by Sovren
            var employmentHistory = theXML.Descendants()
                 .Where(e => e.Name.LocalName == "SchoolOrInstitution")
                 .ToList();

            // Iterate over their emplyment history  
            foreach (XElement node in employmentHistory)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(node.ToString());
                string defaultXlms = doc.DocumentElement.NamespaceURI;
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                namespaceManager.AddNamespace("hrxml", defaultXlms);
                bool isCurrent = false;
                // Parse attendance start date  
                DateTime startDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:Degree/hrxml:DatesOfAttendance/hrxml:StartDate", namespaceManager), ref isCurrent);
                // Parse attendance end date 
                DateTime endDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:Degree/hrxml:DatesOfAttendance/hrxml:EndDate", namespaceManager), ref isCurrent);
                // Parse degree date 
                DateTime degreeDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:Degree/hrxml:DegreeDate", namespaceManager), ref isCurrent);
                string educationalInstitution = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:School/hrxml:SchoolName", namespaceManager));
                string educationalDegreeType = HrXmlNodeAttribute(doc.SelectSingleNode("//hrxml:Degree", namespaceManager), "degreeType");
                string educationalDegree = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:Degree/hrxml:DegreeName", namespaceManager)); ;
                SubscriberEducationHistoryDto educationHistory = new SubscriberEducationHistoryDto()
                {
                    CreateDate = DateTime.UtcNow,
                    ModifyDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ModifyGuid = Guid.Empty,
                    IsDeleted = 0,
                    StartDate = startDate,
                    EndDate = endDate,
                    EducationalInstitution = educationalInstitution,
                    EducationalDegreeType = educationalDegreeType,
                    EducationalDegree = educationalDegree,
                    SubscriberEducationHistoryGuid = Guid.NewGuid()
                };
                rVal.Add(educationHistory);
            }
            return rVal;
        }


        static public List<SubscriberWorkHistoryDto> ParseWorkHistoryFromHrXml(string xml)
        {
            List<SubscriberWorkHistoryDto> rVal = new List<SubscriberWorkHistoryDto>();

            XElement theXML = XElement.Parse(xml);
            // Get list of skill found by Sovren
            List<XElement> employmentHistory = theXML.Descendants()
                 .Where(e => e.Name.LocalName == "EmployerOrg")
                 .ToList();

            // Iterate over their emplyment history  
            foreach (XElement node in employmentHistory)
            {

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(node.ToString());
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
                string defaultXlms = doc.DocumentElement.NamespaceURI;
                namespaceManager.AddNamespace("hrxml", defaultXlms);
                string company = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:EmployerOrgName", namespaceManager));

                var positionHistory = node.Descendants()
                   .Where(e => e.Name.LocalName == "PositionHistory")
                   .ToList();

                foreach (XElement position in positionHistory)
                {
                    doc.LoadXml(position.ToString());
                    bool isCurrent = false;
                    // Parse position start date  
                    DateTime startDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:StartDate", namespaceManager), ref isCurrent);
                    // Parse position end date 
                    DateTime endDate = ParseDateFromHrXmlDate(doc.SelectSingleNode("//hrxml:EndDate", namespaceManager), ref isCurrent);
                    string jobTitle = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:Title", namespaceManager));
                    string jobDescription = HrXmlNodeInnerText(doc.SelectSingleNode("//hrxml:Description", namespaceManager));
                    SubscriberWorkHistoryDto workHistory = new SubscriberWorkHistoryDto()
                    {
                        CreateDate = DateTime.UtcNow,
                        ModifyDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        ModifyGuid = Guid.Empty,
                        IsDeleted = 0,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsCurrent = isCurrent ? 1 : 0,
                        Company = company,
                        JobDescription = jobDescription,
                        Title = jobTitle
                    };
                    rVal.Add(workHistory);
                }
            }
            return rVal;
        }


        static public string HrXmlNodeInnerText(XmlNode node)
        {
            string rVal = string.Empty;
            try
            {
                if (node != null)
                {
                    rVal = node.InnerText;
                }
                return rVal;
            }
            catch
            {
                return string.Empty;
            }
        }



        static public string HrXmlNodeAttribute(XmlNode node, string attribute)
        {
            string rVal = string.Empty;
            try
            {
                if (node != null)
                {
                    XmlElement el = node as XmlElement;
                    if (node.Attributes != null && el.HasAttribute(attribute))
                        rVal = el.Attributes[attribute].Value;
                }
                return rVal;
            }
            catch
            {
                return string.Empty;
            }
        }

        static public DateTime ParseDateFromHrXmlDate(XmlNode hrXMLDate, ref bool isCurrent)
        {

            DateTime date = DateTime.MinValue;
            if (hrXMLDate == null)
                return date;

            isCurrent = false;
            string dateString = hrXMLDate.FirstChild.InnerText;
            switch (hrXMLDate.FirstChild.Name)
            {
                case "YearMonth":
                    date = ParseDateFromHrXmlYearMonthTag(dateString);
                    break;
                case "AnyDate":
                    date = ParseDateFromHrXmlYearMonthTag(dateString);
                    break;
                case "StringDate":
                    date = ParseDateFromHrXmlStringDateTag(dateString, ref isCurrent);
                    break;
                case "Year":
                    date = ParseDateFromHrXmlYearTag(dateString);
                    break;
                default:
                    break;

            }
            return date;
        }


        static public DateTime ParseDateFromHrXmlStringDateTag(string dateStr, ref bool isCurrent)
        {
            try
            {
                if (dateStr == "current")
                {
                    isCurrent = true;
                    return DateTime.MinValue;
                }
                // Try and parse date from StringDate tag.  This tag does not have a well defined format
                // so it probably will not parse in most cases
                return DateTime.Parse(dateStr);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }



        static public DateTime ParseDateFromHrXmlYearTag(string dateStr)
        {
            try
            {
                int year = int.Parse(dateStr);

                return new DateTime(year, 1, 1);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }




        static public DateTime ParseDateFromHrXmlYearMonthTag(string dateStr)
        {
            try
            {
                string[] dateInfo = dateStr.Split('-');
                int year = int.Parse(dateInfo[0]);
                int month = int.Parse(dateInfo[1]);

                return new DateTime(year, month, 1);
            }
            catch
            {
                return DateTime.MinValue;
            }
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

        public static DateTime ToMidnight(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }


        static public string RemoveNewlines(string Str)
        {
            string rVal = Regex.Replace(Str, "\r\n", String.Empty);
            return Regex.Replace(rVal, "\\n", String.Empty);
        }

        static public string RemoveRedundantSpaces(string Str)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(Str.Trim(), " ");

        }

        public static DateTime FromUnixTimeInSeconds(long wozTime)
        {
            return epoch.AddSeconds(wozTime);
        }

        public static long ToUnixTimeInSeconds(DateTime dateTime)
        {
            return (long)(dateTime - epoch).TotalSeconds;
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

        /// <summary>
        /// convert the given string into the specified type 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static dynamic ToType(Type type, string val)
        {
            try
            {
                if (type == typeof(int?) || type == typeof(int))
                {
                    return int.Parse(val);
                }
                else if (type == typeof(string))
                {
                    return val;
                }
                else if (type == typeof(double?) || type == typeof(double))
                {
                    return double.Parse(val);
                }
                else if (type == typeof(DateTime?) || type == typeof(DateTime))
                {
                    return DateTime.Parse(val);
                }
                else if (type == typeof(Guid?) || type == typeof(Guid))
                {
                    return Guid.Parse(val);
                }
                else if (type == typeof(bool?) || type == typeof(bool))
                {
                    return bool.Parse(val);
                }
                else if (type == typeof(long?) || type == typeof(long))
                {
                    return long.Parse(val);
                }
                else if (type == typeof(float?) || type == typeof(float))
                {
                    return float.Parse(val);
                }
                else if (type == typeof(decimal?) || type == typeof(decimal))
                {
                    return decimal.Parse(val);
                }
                else
                {
                    return null;
                }

            }
            catch
            {
                return null;
            }
        }

        public static dynamic ToTypeNullValue(Type type)
        {
            try
            {
                if (type == typeof(int?) || type == typeof(double?) || type == typeof(DateTime?) || type == typeof(Guid?) ||
                    type == typeof(bool?) || type == typeof(long?) || type == typeof(float?) || type == typeof(decimal?)
                    )
                    return null;

                if (type == typeof(int))
                {
                    return 0;
                }
                else if (type == typeof(string))
                {
                    return string.Empty;
                }
                else if (type == typeof(double))
                {
                    return (double)0;
                }
                else if (type == typeof(DateTime))
                {
                    return DateTime.MinValue;
                }
                else if (type == typeof(Guid))
                {
                    return Guid.Empty;
                }
                else if (type == typeof(bool))
                {
                    return false;
                }
                else if (type == typeof(long))
                {
                    return (long)0;
                }
                else if (type == typeof(float))
                {
                    return (float)0;
                }
                else if (type == typeof(decimal))
                {
                    return (decimal)0;
                }
                else
                {
                    return null;
                }

            }
            catch
            {
                return null;
            }
        }



        public static string ToTitleCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            return ti.ToTitleCase(value.ToLower());
        }



        public static string RemoveNonNumericCharacters(string val)
        {
            if (string.IsNullOrEmpty(val))
                return string.Empty;

            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(val, "");
        }
        [Obsolete("Remove this once we are certain we cannot make use of it", false)]
        public static string FormattedDateRange(DateTime? startDate, DateTime? endDate)
        {
            string formattedDateRange = string.Empty;
            if (!startDate.HasValue || startDate.Value == DateTime.MinValue)
            {
                return "No date range specified";
            }
            else
            {
                formattedDateRange = startDate.Value.ToString("MMMM yyyy") + " - ";
            }
            DateTime effectiveEndDate;
            if (!endDate.HasValue || endDate.Value == DateTime.MinValue)
            {
                effectiveEndDate = DateTime.UtcNow;
                formattedDateRange += "Present";
            }
            else
            {
                effectiveEndDate = endDate.Value;
                formattedDateRange += endDate.Value.ToString("MMMM yyyy");
            }
            DateTime period;
            if (effectiveEndDate > startDate.Value)
            {
                period = new DateTime(effectiveEndDate.Subtract(startDate.Value).Ticks);
            }
            else
            {
                period = DateTime.MinValue;
            }
            formattedDateRange += " (" + period.Year + " years " + period.Month + " months)";
            return formattedDateRange;
        }
        [Obsolete("Remove this once we are certain we cannot make use of it", false)]
        public static string FormattedCompensation(string compensationType, decimal compensation)
        {
            string formattedCompensation = string.Empty;
            if (compensation == 0)
            {
                return "No compensation specified";
            }
            else
            {
                formattedCompensation = $"{compensation:C}";
                if (!string.IsNullOrWhiteSpace(compensationType))
                {
                    formattedCompensation += $" ({compensationType})";
                }
            }
            return formattedCompensation;
        }

        // TODO - Remove this function and its usage (campaign landing pages) 
        //        once we're formatting the descriptions received from vendors.
        public static string FormatWozDescriptionFields(string description)
        {
            return description.Replace("Description:", "<strong>Description:</strong> ")
                .Replace("Objectives:", "<br /><br /><strong>Objectives:</strong> ");
        }

        public static byte[] StreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        #region State conversion for front-end display
        public static string GetState(State state)
        {
            switch (state)
            {
                case State.AL:
                    return "ALABAMA";

                case State.AK:
                    return "ALASKA";

                case State.AS:
                    return "AMERICAN SAMOA";

                case State.AZ:
                    return "ARIZONA";

                case State.AR:
                    return "ARKANSAS";

                case State.CA:
                    return "CALIFORNIA";

                case State.CO:
                    return "COLORADO";

                case State.CT:
                    return "CONNECTICUT";

                case State.DE:
                    return "DELAWARE";

                case State.DC:
                    return "DISTRICT OF COLUMBIA";

                case State.FM:
                    return "FEDERATED STATES OF MICRONESIA";

                case State.FL:
                    return "FLORIDA";

                case State.GA:
                    return "GEORGIA";

                case State.GU:
                    return "GUAM";

                case State.HI:
                    return "HAWAII";

                case State.ID:
                    return "IDAHO";

                case State.IL:
                    return "ILLINOIS";

                case State.IN:
                    return "INDIANA";

                case State.IA:
                    return "IOWA";

                case State.KS:
                    return "KANSAS";

                case State.KY:
                    return "KENTUCKY";

                case State.LA:
                    return "LOUISIANA";

                case State.ME:
                    return "MAINE";

                case State.MH:
                    return "MARSHALL ISLANDS";

                case State.MD:
                    return "MARYLAND";

                case State.MA:
                    return "MASSACHUSETTS";

                case State.MI:
                    return "MICHIGAN";

                case State.MN:
                    return "MINNESOTA";

                case State.MS:
                    return "MISSISSIPPI";

                case State.MO:
                    return "MISSOURI";

                case State.MT:
                    return "MONTANA";

                case State.NE:
                    return "NEBRASKA";

                case State.NV:
                    return "NEVADA";

                case State.NH:
                    return "NEW HAMPSHIRE";

                case State.NJ:
                    return "NEW JERSEY";

                case State.NM:
                    return "NEW MEXICO";

                case State.NY:
                    return "NEW YORK";

                case State.NC:
                    return "NORTH CAROLINA";

                case State.ND:
                    return "NORTH DAKOTA";

                case State.MP:
                    return "NORTHERN MARIANA ISLANDS";

                case State.OH:
                    return "OHIO";

                case State.OK:
                    return "OKLAHOMA";

                case State.OR:
                    return "OREGON";

                case State.PW:
                    return "PALAU";

                case State.PA:
                    return "PENNSYLVANIA";

                case State.PR:
                    return "PUERTO RICO";

                case State.RI:
                    return "RHODE ISLAND";

                case State.SC:
                    return "SOUTH CAROLINA";

                case State.SD:
                    return "SOUTH DAKOTA";

                case State.TN:
                    return "TENNESSEE";

                case State.TX:
                    return "TEXAS";

                case State.UT:
                    return "UTAH";

                case State.VT:
                    return "VERMONT";

                case State.VI:
                    return "VIRGIN ISLANDS";

                case State.VA:
                    return "VIRGINIA";

                case State.WA:
                    return "WASHINGTON";

                case State.WV:
                    return "WEST VIRGINIA";

                case State.WI:
                    return "WISCONSIN";

                case State.WY:
                    return "WYOMING";
            }

            throw new Exception("Not Available");
        }



        public static State GetStateByName(string name)
        {
            switch (name.ToUpper())
            {
                case "ALABAMA":
                    return State.AL;

                case "ALASKA":
                    return State.AK;

                case "AMERICAN SAMOA":
                    return State.AS;

                case "ARIZONA":
                    return State.AZ;

                case "ARKANSAS":
                    return State.AR;

                case "CALIFORNIA":
                    return State.CA;

                case "COLORADO":
                    return State.CO;

                case "CONNECTICUT":
                    return State.CT;

                case "DELAWARE":
                    return State.DE;

                case "DISTRICT OF COLUMBIA":
                    return State.DC;

                case "FEDERATED STATES OF MICRONESIA":
                    return State.FM;

                case "FLORIDA":
                    return State.FL;

                case "GEORGIA":
                    return State.GA;

                case "GUAM":
                    return State.GU;

                case "HAWAII":
                    return State.HI;

                case "IDAHO":
                    return State.ID;

                case "ILLINOIS":
                    return State.IL;

                case "INDIANA":
                    return State.IN;

                case "IOWA":
                    return State.IA;

                case "KANSAS":
                    return State.KS;

                case "KENTUCKY":
                    return State.KY;

                case "LOUISIANA":
                    return State.LA;

                case "MAINE":
                    return State.ME;

                case "MARSHALL ISLANDS":
                    return State.MH;

                case "MARYLAND":
                    return State.MD;

                case "MASSACHUSETTS":
                    return State.MA;

                case "MICHIGAN":
                    return State.MI;

                case "MINNESOTA":
                    return State.MN;

                case "MISSISSIPPI":
                    return State.MS;

                case "MISSOURI":
                    return State.MO;

                case "MONTANA":
                    return State.MT;

                case "NEBRASKA":
                    return State.NE;

                case "NEVADA":
                    return State.NV;

                case "NEW HAMPSHIRE":
                    return State.NH;

                case "NEW JERSEY":
                    return State.NJ;

                case "NEW MEXICO":
                    return State.NM;

                case "NEW YORK":
                    return State.NY;

                case "NORTH CAROLINA":
                    return State.NC;

                case "NORTH DAKOTA":
                    return State.ND;

                case "NORTHERN MARIANA ISLANDS":
                    return State.MP;

                case "OHIO":
                    return State.OH;

                case "OKLAHOMA":
                    return State.OK;

                case "OREGON":
                    return State.OR;

                case "PALAU":
                    return State.PW;

                case "PENNSYLVANIA":
                    return State.PA;

                case "PUERTO RICO":
                    return State.PR;

                case "RHODE ISLAND":
                    return State.RI;

                case "SOUTH CAROLINA":
                    return State.SC;

                case "SOUTH DAKOTA":
                    return State.SD;

                case "TENNESSEE":
                    return State.TN;

                case "TEXAS":
                    return State.TX;

                case "UTAH":
                    return State.UT;

                case "VERMONT":
                    return State.VT;

                case "VIRGIN ISLANDS":
                    return State.VI;

                case "VIRGINIA":
                    return State.VA;

                case "WASHINGTON":
                    return State.WA;

                case "WEST VIRGINIA":
                    return State.WV;

                case "WISCONSIN":
                    return State.WI;

                case "WYOMING":
                    return State.WY;
            }

            throw new Exception("Not Available");
        }

        public enum State
        {
            AL,
            AK,
            AS,
            AZ,
            AR,
            CA,
            CO,
            CT,
            DE,
            DC,
            FM,
            FL,
            GA,
            GU,
            HI,
            ID,
            IL,
            IN,
            IA,
            KS,
            KY,
            LA,
            ME,
            MH,
            MD,
            MA,
            MI,
            MN,
            MS,
            MO,
            MT,
            NE,
            NV,
            NH,
            NJ,
            NM,
            NY,
            NC,
            ND,
            MP,
            OH,
            OK,
            OR,
            PW,
            PA,
            PR,
            RI,
            SC,
            SD,
            TN,
            TX,
            UT,
            VT,
            VI,
            VA,
            WA,
            WV,
            WI,
            WY
        }

        #endregion
    }
}
