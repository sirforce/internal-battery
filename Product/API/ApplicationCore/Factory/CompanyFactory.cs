﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CompanyFactory
    {

        static public Company CreateCompany(string companyName)
        {
            Company rVal = new Company();
            rVal.CompanyName = companyName;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.Empty;
            rVal.ModifyDate = DateTime.UtcNow;
            rVal.ModifyGuid = Guid.Empty;
            rVal.CompanyGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public async Task<Company> GetOrAdd(IRepositoryWrapper repositoryWrapper, string companyName)
        {            
            companyName = companyName.Trim();
            Company company = repositoryWrapper.Company.GetAllWithTracking()
                .Where(c => c.IsDeleted == 0 && c.CompanyName == companyName)
                .FirstOrDefault();

            if (company == null)
            {
                company = CreateCompany(companyName);
                await repositoryWrapper.Company.Create(company);
                await repositoryWrapper.Company.SaveAsync();
            }
            return company;
        }

        static public async Task<Company>GetCompanyByGuid(IRepositoryWrapper repositoryWrapper, Guid CompanyGuid)
        {
 
            Company company = await repositoryWrapper.Company.GetAllWithTracking()
                .Where(c => c.IsDeleted == 0 && c.CompanyGuid == CompanyGuid)
                .FirstOrDefaultAsync();    
            return company;
        }

        static public async Task<Company> GetCompanyByCompanyName(IRepositoryWrapper repositoryWrapper, string  CompanyName)
        {

            Company company = await repositoryWrapper.Company.GetAllWithTracking()
                .Where(c => c.IsDeleted == 0 && c.CompanyName == CompanyName)
                .FirstOrDefaultAsync();
            return company;
        }




    }
}
