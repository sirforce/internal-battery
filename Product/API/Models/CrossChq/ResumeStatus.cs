﻿using System;

namespace UpDiddyApi.Models.CrossChq
{
    public class CrossChqResumeStatus
    {
        public Guid SubscriberGuid { get; set; }

        public Guid ProfileGuid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime SubscriberCreateDate { get; set; }

        public DateTime? ResumeUploadedDate { get; set; }

        public int? ElapsedHoursToUploadResume { get; set; }

        public string CrossChqReferenceCheckType { get; set; }

        public string CrossChqJobTitle { get; set; }

        public DateTime? CrossChqStatusDate { get; set; }

        public int? CrossChqPercentage { get; set; }

        public string CrossChqStatus { get; set; }

        public int TotalRecords { get; set; }
    }
}
