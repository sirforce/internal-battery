﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;


namespace UpDiddy.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        public IList<SkillDto> Skills { get; set; }
        public string SelectedSkills { get; set; }
        public IList<EnrollmentDto> Enrollments { get; set; }
        public IEnumerable<SelectListItem> States { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }
        public IList<SubscriberFileDto> Files { get; set; }
        public Guid? SelectedState { get; set; }
        public Guid? SelectedCountry { get; set; }
        [RegularExpression(@"^[ a-zA-Z'-]+$", ErrorMessage = "First name may only contain alphabetic characters, spaces, apostrophes, and hyphens.")]
        public string FirstName { get; set; }
        [RegularExpression(@"^[ a-zA-Z'-]+$", ErrorMessage = "Last name may only contain alphabetic characters, spaces, apostrophes, and hyphens.")]
        public string LastName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        private string _FormattedPhone;

        // todo: refactor for future use
        public SubscriberFileDto Resume
        {
            get
            {
                if (Files == null || Files.Count == 0)
                    return null;

                // return the last record since we are only tracking one resume at a time for now
                return Files[Files.Count - 1];
            }
        }
        public string FormattedPhone
        {
            get
            {
                long phone = 0;
                if (long.TryParse(this._FormattedPhone, out phone))
                    return String.Format("{0:(###) ###-####}", phone);
                else
                    return _FormattedPhone;
            }
            set
            {
                if (value != null)
                    _FormattedPhone = value.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                else
                    _FormattedPhone = value;
            }
        }

        [RegularExpression("^([0-9]{0,3})?[2-9]{1}[0-9]{9}$", ErrorMessage = "Phone must be 10 digits and may not start with a 0 or 1. Please edit your phone number before continuing.")]
        public string Phone
        {
            get
            {
                if (this._FormattedPhone != null)
                    return this.FormattedPhone.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty);
                else
                    return _FormattedPhone;
            }
        }
        // no validation data annotation required for email since this has been validated by MSAL and cannot be modified by the user
        public string Email { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?facebook.com/[A-z0-9_]+/?$", ErrorMessage = "The Facebook profile URL is not valid.")]
        public string FacebookUrl { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?linkedin.com/in/[A-z0-9_-]+/?$", ErrorMessage = "The LinkedIn profile URL is not valid.")]
        public string LinkedInUrl { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?twitter.com/[A-z0-9_]+/?$", ErrorMessage = "The Twitter profile URL is not valid.")]
        public string TwitterUrl { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?stackoverflow.com/users/[0-9]+/[A-z0-9_]+/?$", ErrorMessage = "The StackOverflow profile URL is not valid.")]
        public string StackOverflowUrl { get; set; }
        [RegularExpression(@"^http(s)?://([\w]+.)?github.com/[A-z0-9_]+/?$", ErrorMessage = "The GitHub profile URL is not valid.")]
        public string GithubUrl { get; set; }
        public Guid? SubscriberGuid { get; set; }

        public Boolean IsAnyProfileInformationPopulated
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(this.FirstName) ||
                    !string.IsNullOrWhiteSpace(this.LastName) ||
                    !string.IsNullOrWhiteSpace(this.Address) ||
                    !string.IsNullOrWhiteSpace(this.City) ||
                    !string.IsNullOrWhiteSpace(this.Phone) ||
                    !string.IsNullOrWhiteSpace(this.FacebookUrl) ||
                    !string.IsNullOrWhiteSpace(this.LinkedInUrl) ||
                    !string.IsNullOrWhiteSpace(this.TwitterUrl) ||
                    !string.IsNullOrWhiteSpace(this.StackOverflowUrl) ||
                    !string.IsNullOrWhiteSpace(this.GithubUrl) ||
                    SelectedState.HasValue;
            }
        }

        public Boolean HasFirstAndLastName
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(this.FirstName) &&
                    !string.IsNullOrWhiteSpace(this.LastName);
            }
        }

        public Boolean HasFullyQualifiedAddress
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(this.Address) &&
                    !string.IsNullOrWhiteSpace(this.City) &&
                    this.SelectedState != null &&
                    this.SelectedState != Guid.Empty;
            }
        }

        public Boolean HasSuppliedAnySocialLinks
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(this.FacebookUrl) ||
                    !string.IsNullOrWhiteSpace(this.TwitterUrl) ||
                    !string.IsNullOrWhiteSpace(this.LinkedInUrl) ||
                    !string.IsNullOrWhiteSpace(this.StackOverflowUrl) ||
                    !string.IsNullOrWhiteSpace(this.GithubUrl);

            }
        }
    }
}
