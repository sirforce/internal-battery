﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class WozCreateStudentDto
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public int acceptedTermsOfServiceDocumentId { get; set; }
        public bool suppressRegistrationEmail { get; set; }
        public string phoneNumberPrimary { get; set; }
    }
}
