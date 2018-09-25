﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
 
namespace UpDiddyApi.Helpers
{
    public class AutoMapperConfiguration
    {
        static public void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Topic, TopicDto>();
                cfg.CreateMap<TopicDto, Topic>();
                cfg.CreateMap<Vendor, VendorDto>();
                cfg.CreateMap<VendorDto, Vendor>();
                cfg.CreateMap<Course, CourseDto>();
                cfg.CreateMap<CourseDto, Course>();
                cfg.CreateMap<Enrollment, EnrollmentDto>();
                cfg.CreateMap<EnrollmentDto, Enrollment>();
                cfg.CreateMap<Subscriber, SubscriberDto>();
                cfg.CreateMap<SubscriberDto, Subscriber>();
                cfg.CreateMap<WozCourseEnrollment, WozCourseEnrollmentDto>();
                cfg.CreateMap<WozCourseEnrollmentDto, WozCourseEnrollment>();
            });

        }

    }
}
