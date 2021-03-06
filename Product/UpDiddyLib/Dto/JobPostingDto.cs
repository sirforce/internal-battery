﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpDiddyLib.Dto
{

    public enum JobPostingStatus { Draft = 0, Active, Expired };

    public class JobPostingDto : BaseDto
    {
        public string RequestId { get; set; }

        public string ClientEventId { get; set; }

        public Guid? JobPostingGuid { get; set; }
 
        public DateTime PostingDateUTC { get; set; }
        
        public DateTime PostingExpirationDateUTC { get; set; }
 
        public DateTime ApplicationDeadlineUTC { get; set; }

        public string CloudTalentUri { get; set; }
        
        public int CloudTalentIndexStatus { get; set; }

        public RecruiterDto Recruiter { get; set; }

        public CompanyDto  Company { get; set; }
     

        public IndustryDto Industry { get; set; }

        public JobCategoryDto JobCategory { get; set; }
 
        public string Title { get; set; }
       
        public string Description { get; set; }
       
        public SecurityClearanceDto SecurityClearance { get; set; }
  
        public EmploymentTypeDto EmploymentType { get; set; } 

        public bool H2Visa { get; set; }

        public bool IsAgencyJobPosting { get; set; }

        public int TelecommutePercentage { get; set; }
        public Decimal Compensation { get; set; }
 

        public CompensationTypeDto CompensationType { get; set; }
        public Boolean ThirdPartyApply { get; set; }
        public string ThirdPartyApplicationUrl { get; set; }
        public string Country { get; set; }
 
        public string City { get; set; }
       
        public string Province { get; set; }
 
        public string PostalCode { get; set; }
 
        public string StreetAddress { get; set; }
        public ExperienceLevelDto ExperienceLevel { get; set; }

        public EducationLevelDto EducationLevel { get; set; }

        public List<SkillDto> JobPostingSkills { get; set; }


        public int IsPrivate { get; set; }

        public int JobStatus { get; set; }

        public string SemanticJobPath { get; set; }
        public string ThirdPartyIdentifier { get; set; }


        


        public string CityProvince
        {
            get
            {
                var array = new string[]{ City, Province };
                return String.Join(", ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        public JobSearchResultDto SimilarJobs { get; set; }

        #region metadata 
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }

        #endregion


    }

}
