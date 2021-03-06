﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Dto;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.ViewModels
{
    public class JobDetailsViewModel : BaseViewModel
    {
        public string ClientEventId { get; set; }
        public Guid? JobPostingFavoriteGuid { get; set; }
        public string RequestId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string PostedDate { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostingId { get; set; }
        public string EmployeeType { get; set; }
        public string Summary { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string ThirdPartyIdentifier { get; set; }
        public Guid? JobPostingGuid { get; set; }
        public string CompanyBoilerplate { get; set; }
        public bool IsThirdPartyJob { get; set; }
        public string LogoUrl { get; set; }
        public List<string> Skills { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public JobSearchResultDto SimilarJobsSearchResult { get; set; }
        public Dictionary<Guid, Guid> SimilarJobsFavorites { get; set; }
        public string ApplyUrl { get; set; }
    }
}
