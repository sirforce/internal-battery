﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class RecruiterCompanyFactory
    {
        public static async Task<List<RecruiterCompany>> GetRecruiterCompanyById(IRepositoryWrapper repositoryWrapper, int subscriberId)
        {
            return await repositoryWrapper.RecruiterCompanyRepository.GetAllWithTracking()
               .Include(s => s.Company)
               .Include(s => s.Recruiter.Subscriber)
               .Where(rc => rc.IsDeleted == 0 && rc.Recruiter.SubscriberId == subscriberId)
               .ToListAsync();
        }

        public static RecruiterCompany CreateRecruiterCompany(int recruiterId, int companyId, bool isStaff)
        {

            RecruiterCompany recruiterCompany = new RecruiterCompany();
            recruiterCompany.CreateDate = DateTime.UtcNow;
            recruiterCompany.CreateGuid = Guid.Empty;
            recruiterCompany.RecruiterId = recruiterId;
            recruiterCompany.CompanyId = companyId;
            recruiterCompany.IsDeleted = 0;
            recruiterCompany.RecruiterCompanyGuid = Guid.NewGuid();
            return recruiterCompany;
        }

        public static async Task<RecruiterCompany> GetOrAdd(IRepositoryWrapper repositoryWrapper, int recruiterId, int companyId, bool isStaff)
        {
            RecruiterCompany recruiterCompany = await repositoryWrapper.RecruiterCompanyRepository.GetAllWithTracking()
                .Include(rc => rc.Recruiter)
                .Include(rc => rc.Company)
                .Where(rc => rc.IsDeleted == 0 && rc.Recruiter.RecruiterId == recruiterId && rc.Company.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (recruiterCompany == null)
            {
                recruiterCompany = CreateRecruiterCompany(recruiterId, companyId, isStaff);
                await repositoryWrapper.RecruiterCompanyRepository.Create(recruiterCompany);
                await repositoryWrapper.SaveAsync();
            }
            return recruiterCompany;
        }
    }
}
