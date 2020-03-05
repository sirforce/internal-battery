﻿using System;
using System.ComponentModel.DataAnnotations;

namespace UpDiddyLib.Domain.Models.G2
{
    public class ProfileDto
    {
        public Guid ProfileGuid { get; set; } 
        [Required]
        public Guid CompanyGuid { get; set; } 
        [Required]
        public Guid SubscriberGuid { get; set; }
        [StringLength(100)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        [StringLength(254)]
        public string Email { get; set; }
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        [StringLength(100)]
        public string StreetAddress { get; set; }
        public Guid? CityGuid { get; set; }
        public Guid? StateGuid { get; set; }
        public Guid? PostalGuid { get; set; }
        public Guid? ContactTypeGuid { get; set; }
        public Guid? ExperienceLevelGuid { get; set; }
        public Guid? EmploymentTypeGuid { get; set; }
        [StringLength(100)]
        public string Title { get; set; }
        public bool? IsWillingToTravel { get; set; }
        public bool? IsActiveJobSeeker { get; set; }
        public bool? IsCurrentlyEmployed { get; set; }
        public bool? IsWillingToWorkProBono { get; set; }
        public decimal? CurrentRate { get; set; }
        public decimal? DesiredRate { get; set; }
        [StringLength(500)]
        public string Goals { get; set; }
        [StringLength(500)]
        public string Preferences { get; set; }
    }
}
