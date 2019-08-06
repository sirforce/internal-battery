﻿using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudTalentSolution = Google.Apis.CloudTalentSolution.v3.Data;
using Google.Protobuf.WellKnownTypes;
using Google.Apis.CloudTalentSolution.v3;
using UpDiddyLib.Helpers;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Factory;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UpDiddyApi.ApplicationCore.Services;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace UpDiddyApi.Helpers.Job
{
    public class JobUrlHelper
    {
        #region Job Urls

        public static string MapFacetToUrl(JobQueryDto query, string facetName, string facetValue, string IndustryUrl, string LocationUrl, string TopLevelDomain)
        {
           
            string rVal = "Unknown Facet!";
            switch (facetName.ToLower())
            {
                case "jobcategory":
                    rVal =  UrlComponentReplace(LocationUrl, 3, facetValue);
                    break;
                case "industry":
                    rVal = UrlComponentReplace(IndustryUrl, 2, facetValue);
                    break;
                case "skills":
                    rVal =  UrlComponentReplace(LocationUrl, 7, facetValue);
                    break;
                case "city":
                    rVal = MapCityFacet(LocationUrl, facetValue);
                    break;
                case "date_published":
                    rVal = MapQueryStringFacet(LocationUrl, "date-published", facetValue);
                    break;
                case "employmenttype":
                    rVal = MapQueryStringFacet(LocationUrl, "employment-type", facetValue);
                    break;
                case "experiencelevel":
                    rVal = MapQueryStringFacet(LocationUrl, "experience-level", facetValue);
                    break;
                case "educationlevel":
                    rVal = rVal = MapQueryStringFacet(LocationUrl, "education-level", facetValue);
                    break;
                case "admin_1":
                    rVal =  UrlComponentReplace(LocationUrl, 3, facetValue);
                    break;
                case "company_display_name":
                    rVal = MapQueryStringFacet(LocationUrl, "companyname", facetValue);
                    break;
                default:
                    break;

            }

            return TopLevelDomain + rVal;
        }
        /// <summary>
        /// edge case of city being returned from talent cloude as city,state
        /// </summary>
        /// <param name="defUrl"></param>
        /// <param name="facetValue"></param>
        /// <returns></returns>
        public static string MapCityFacet( string defUrl, string facetValue)
        {
            string rVal = string.Empty;
            string [] info = facetValue.Split(",");
            rVal = UrlComponentReplace(defUrl, 4, FacetQueryParam(info[0]) );
            rVal = UrlComponentReplace(rVal, 3, FacetQueryParam(info[1]));

            return rVal;

        }

        public static string MapQueryStringFacet(string defUrl, string queryStringName, string facetValue )
        {
            StringBuilder rVal = new StringBuilder();
            rVal.Append(defUrl);
            rVal.Append("?");
            rVal.Append(queryStringName);
            rVal.Append("=");
            rVal.Append(FacetQueryParam(facetValue));
            return rVal.ToString();
        }
      
        #endregion

        #region Default Urls 

        static public string GetDefaultLocationUrl(string urlPrefix, JobQueryDto jobQueryDto)
        {
            StringBuilder rVal = new StringBuilder();
            rVal.Append(urlPrefix);

            if (string.IsNullOrEmpty(jobQueryDto.Country) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.Country));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.Province) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.Province));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.City) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.City));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.Industry) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.Industry));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.JobCategory) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.JobCategory));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.Skill) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.Skill));
            else
                rVal.Append("all");

            rVal.Append("/0");

            return rVal.ToString();
        }


        static public string GetDefaultIndustryUrl(string urlPrefix, JobQueryDto jobQueryDto)
        {
            StringBuilder rVal = new StringBuilder();

            rVal.Append(urlPrefix);

            if (string.IsNullOrEmpty(jobQueryDto.Industry) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.Industry));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.JobCategory) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.JobCategory));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.Country) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.Country));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.Province) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.Province));
            else
                rVal.Append("all");
            rVal.Append("/");

            if (string.IsNullOrEmpty(jobQueryDto.City) == false)
                rVal.Append(FacetQueryParam(jobQueryDto.City));
            else
                rVal.Append("all");
            rVal.Append("/0");

            return rVal.ToString();
        }


        #endregion

        #region Helper functions 

        /// <summary>
        /// replace the nth component in the url with a new value 
        /// todo find a more efficient way to do this.... 
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="index"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        static private string UrlComponentReplace ( string Url, int index, string newValue)
        {            
            StringBuilder rVal = new StringBuilder();

            string[] urlComponents = Url.Split('/');
            int currentIndex = 0;
            foreach (string s in urlComponents)
            {
                if ( currentIndex > 0 )
                    rVal.Append("/");

                if (currentIndex == index)
                    rVal.Append(FacetQueryParam(newValue));
                else
                    rVal.Append(s);                
                ++currentIndex;
            }
            return rVal.ToString();
        }

        /// <summary>
        ///  UrlEncode facet values 
        /// </summary>
        /// <param name="facetInfo"></param>
        /// <returns></returns>
        static private string FacetQueryParam(string facetInfo)
        {
            return WebUtility.UrlEncode(facetInfo.Trim().ToLower());
        }

        #endregion

    }
}
