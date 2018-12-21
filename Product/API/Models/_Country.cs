﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpDiddyApi.Models
{
    public partial class Country
    {
        public static Country GetCountryByCountryCode(UpDiddyDbContext db, string countryCode)
        {
            return db.Country
                .Where(s => s.IsDeleted == 0 && s.Code2 == countryCode.Trim())
                .FirstOrDefault();
        }
    }
}
