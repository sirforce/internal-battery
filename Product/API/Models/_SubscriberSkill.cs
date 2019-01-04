﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UpDiddyApi.Business.Resume;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.Models
{
    public partial class SubscriberSkill : BaseModel
    {
        #region Factory Methods 

        public static bool AddSkillForSubscriber(UpDiddyDbContext db, Subscriber subscriber, Skill skill)
        {
            bool rVal = true;
            try
            {
                // check for a matching skill for this subscriber which was logically deleted
                var deletedSubscriberSkill = db.SubscriberSkill
                    .Where(ss => ss.IsDeleted == 1 && ss.SubscriberId == subscriber.SubscriberId && ss.SkillId == skill.SkillId)
                    .FirstOrDefault();

                if (deletedSubscriberSkill != null)
                {
                    // if the skill was logically deleted, remove that flag and mark it as modified
                    deletedSubscriberSkill.IsDeleted = 0;
                    deletedSubscriberSkill.ModifyDate = DateTime.UtcNow;
                    db.SubscriberSkill.Update(deletedSubscriberSkill);
                }
                else
                {
                    SubscriberSkill subscriberSkill = new SubscriberSkill()
                    {
                        SkillId = skill.SkillId,
                        SubscriberId = subscriber.SubscriberId,
                        CreateDate = DateTime.UtcNow,
                        CreateGuid = Guid.Empty,
                        ModifyDate = DateTime.UtcNow,
                        ModifyGuid = Guid.Empty,
                        IsDeleted = 0,
                        SubscriberSkillGuid = Guid.NewGuid()
                    };
                    db.SubscriberSkill.Add(subscriberSkill);
                }
                db.SaveChanges();
            }
            catch
            {
                rVal = false;
            }
            return rVal;
        }

        public static SubscriberSkill GetSkillForSubscriber(UpDiddyDbContext db, Subscriber subscriber, Skill skill)
        {
            return db.SubscriberSkill
                .Where(ss => ss.IsDeleted == 0 && ss.SkillId == skill.SkillId && ss.SubscriberId == subscriber.SubscriberId)
                .FirstOrDefault();
        }

        #endregion
    }
}
