﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
namespace UpDiddyApi.ApplicationCore.Factory
{
    public class SkillFactory
    {
        private const string allSkillsCacheKey = "AllSkills";
        
        public static Skill CreateSkill(string skillName)
        {
            Skill rVal = new Skill();
            rVal.SkillName = skillName;
            rVal.CreateDate = DateTime.UtcNow;
            rVal.CreateGuid = Guid.Empty;
            rVal.SkillGuid = Guid.NewGuid();
            rVal.IsVerified = true;
            rVal.IsDeleted = 0;
            return rVal;
        }

        static public async Task<Skill> GetOrAdd(IRepositoryWrapper repositoryWrapper, IMemoryCacheService memoryCacheService, string skillName)
        {
            skillName = skillName.Trim().ToLower();

            Skill skill = repositoryWrapper.SkillRepository.GetAllWithTracking()
                .Where(s => s.IsDeleted == 0 && s.SkillName == skillName)
                .FirstOrDefault();

            if (skill == null)
            {
                skill = CreateSkill(skillName);
                await repositoryWrapper.SkillRepository.Create(skill);
                await repositoryWrapper.SkillRepository.SaveAsync();
                memoryCacheService.ClearCacheByKey(allSkillsCacheKey);
            }
            return skill;
        }


        static public async Task<Skill> GetSkillByGuid(IRepositoryWrapper repositoryWrapper, Guid skillGuid)
        {
            return await repositoryWrapper.SkillRepository.GetAllWithTracking()
               .Where(s => s.IsDeleted == 0 && s.SkillGuid == skillGuid)
               .FirstOrDefaultAsync();
        }
    }
}