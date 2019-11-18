﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Domain.Models
{
    public class SubscribeProfileBasicDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }


        public Guid SubscriberGuid { get; set; }

        public bool IsAgreeToMarketingEmails { get; set; }

        public string Auth0UserId { get; set; }

        public string Email { get; set; }

        public string ProvinceCode { get; set; }
        public string City { get; set; }

        public string PostalCode { get; set; }

        public string Address { get; set; }
               
    }
}