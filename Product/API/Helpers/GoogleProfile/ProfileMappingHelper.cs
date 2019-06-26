﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models;
using UpDiddyLib.Helpers;
using Google.Protobuf.WellKnownTypes;


namespace UpDiddyApi.Helpers.GoogleProfile
{
    public class ProfileMappingHelper
    {

        #region CC subscriber -> cloud talent profile  mapping helpers
        static public GoogleCloudProfile CreateGoogleProfile(UpDiddyDbContext db, Subscriber subscriber, IList<SubscriberSkill> skills)
        {

            GoogleCloudProfile gcp = new GoogleCloudProfile()
            {
                name = subscriber.CloudTalentUri,
                externalId = subscriber.SubscriberGuid.ToString(),
                createTime = new Timestamp()
                {
                    Seconds = Utils.ToUnixTimeInSeconds(subscriber.CreateDate),
                    Nanos = 0

                },
                updateTime = new Timestamp()
                {
                    Seconds = Utils.ToUnixTimeInSeconds(subscriber.ModifyDate.Value),
                    Nanos = 0
                },
                personNames = MapPersonName(subscriber),
                addresses = MapAddress(subscriber),
                skills = MapSkill(skills),
                employmentRecords = MapWorkHistory(subscriber),
                educationRecords = MapEducationHistory(subscriber),
            };


   



            return gcp;
        }

        #endregion

        #region helper functions 

        static private List<PersonName> MapPersonName(Subscriber subscriber)
        {
            List <PersonName> rVal = new List<PersonName>();
            PersonName personName = new PersonName(subscriber);
            rVal.Add(personName);
            return rVal;
        }

        static private List<Address> MapAddress(Subscriber subscriber)
        {
            List<Address> rVal = new List<Address>();
            Address address = new Address(subscriber);
            rVal.Add(address);
            return rVal;
        }

        static private List<Skill> MapSkill(IList<SubscriberSkill> skills)
        {
            List<Skill> rVal = new List<Skill>();
            foreach (SubscriberSkill ss in skills)
            {
                Skill skill = new Skill(ss);
                rVal.Add(skill);
            }            
            return rVal;
        }

        static private List<EmploymentRecord> MapWorkHistory(Subscriber subscriber)
        {
            if (subscriber.SubscriberWorkHistory == null || subscriber.SubscriberWorkHistory.Count == 0)
                return null;

            List<EmploymentRecord> rVal = new List<EmploymentRecord>();
            foreach (SubscriberWorkHistory swh in subscriber.SubscriberWorkHistory)
            {
                EmploymentRecord employmentRecord = new EmploymentRecord(swh);
                rVal.Add(employmentRecord);
            }
            return rVal;
        }

        static private List<EducationRecord> MapEducationHistory(Subscriber subscriber)
        {
            if (subscriber.SubscriberEducationHistory == null || subscriber.SubscriberEducationHistory.Count == 0)
                return null;

            List<EducationRecord> rVal = new List<EducationRecord>();
            foreach (SubscriberEducationHistory seh in subscriber.SubscriberEducationHistory)
            {
                EducationRecord educationRecord = new EducationRecord(seh);
                rVal.Add(educationRecord);
            }
            return rVal;
        }



        #endregion



    }





}
