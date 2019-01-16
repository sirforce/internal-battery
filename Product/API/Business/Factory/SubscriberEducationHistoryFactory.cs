﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;

namespace UpDiddyApi.Business.Factory
{
    public class SubscriberEducationHistoryFactory
    {
        public static SubscriberEducationHistory GetEducationHistoryForSubscriber(UpDiddyDbContext db, Subscriber subscriber, EducationalInstitution educationalInstitution, EducationalDegree educationalDegree, DateTime startDate, DateTime endDate, DateTime degreeDate)
        {
            return db.SubscriberEducationHistory
                .Where(eh => eh.IsDeleted == 0 && eh.EducationalDegreeId == educationalDegree.EducationalDegreeId &&
                    eh.EducationalInstitutionId == educationalInstitution.EducationalInstitutionId && eh.SubscriberId == subscriber.SubscriberId &&
                    eh.StartDate == startDate && eh.EndDate == endDate && eh.DegreeDate == degreeDate)
                .FirstOrDefault();
        }

        public static bool AddEducationHistoryForSubscriber(UpDiddyDbContext db, Subscriber subscriber, SubscriberEducationHistoryDto educationkHistory,
                EducationalInstitution educationalInstitution, EducationalDegree educationalDegree, EducationalDegreeType educationalDegreeType)
        {
            bool rVal = true;
            try
            {
                SubscriberEducationHistory eh = new SubscriberEducationHistory()
                {
                    StartDate = educationkHistory.StartDate,
                    EndDate = educationkHistory.EndDate,
                    SubscriberId = subscriber.SubscriberId,
                    CreateDate = DateTime.UtcNow,
                    CreateGuid = Guid.Empty,
                    ModifyDate = DateTime.UtcNow,
                    ModifyGuid = Guid.Empty,
                    SubscriberEducationHistoryGuid = Guid.NewGuid(),
                    IsDeleted = 0,
                    EducationalInstitutionId = educationalInstitution.EducationalInstitutionId,
                    DegreeDate = educationkHistory.DegreeDate,
                    EducationalDegreeId = educationalDegree.EducationalDegreeId,
                    EducationalDegreeTypeId = educationalDegreeType.EducationalDegreeTypeId
                };
                db.SubscriberEducationHistory.Add(eh);
                db.SaveChanges();
            }
            catch
            {
                rVal = false;
            }
            return rVal;
        }
    }
}
