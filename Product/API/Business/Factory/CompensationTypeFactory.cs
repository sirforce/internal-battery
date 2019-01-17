﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;

namespace UpDiddyApi.Business.Factory
{
    public class CompensationTypeFactory
    {
        public static CompensationType  GetCompensationTypeByName(UpDiddyDbContext db, string CompensationTypeName)
        {
            return db.CompensationType
                .Where(s => s.IsDeleted == 0 && s.CompensationTypeName == CompensationTypeName)
                .FirstOrDefault();
        }

        static public CompensationType CreateCompensationType(string CompensationTypeName)
        {
            CompensationType rVal = new CompensationType();
            rVal.CompensationTypeName = CompensationTypeName;
            rVal.CreateDate = DateTime.Now;
            rVal.CreateGuid = Guid.NewGuid();
            rVal.ModifyDate = DateTime.Now;
            rVal.ModifyGuid = Guid.NewGuid();
            rVal.IsDeleted = 0;
            return rVal;
        }


        static public CompensationType GetOrAdd(UpDiddyDbContext db, string CompensationTypeName)
        {
            CompensationTypeName = CompensationTypeName.Trim();
            CompensationType compensatopnType = db.CompensationType
                .Where(c => c.IsDeleted == 0 && c.CompensationTypeName == CompensationTypeName)
                .FirstOrDefault();

            if (compensatopnType == null)
            {
                compensatopnType = CreateCompensationType(CompensationTypeName);
                db.CompensationType.Add(compensatopnType);
                db.SaveChanges();
            }
            return compensatopnType;
        }

    }
}
