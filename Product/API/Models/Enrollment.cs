﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpDiddyApi.ApplicationCore.Factory;
namespace UpDiddyApi.Models
{
    public class Enrollment : BaseModel
    {
        public int EnrollmentId { get; set; }
        public Guid? EnrollmentGuid { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public int SubscriberId { get; set; }
        public Subscriber Subscriber { get; set; }
        public DateTime DateEnrolled { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public Decimal PricePaid { get; set; }
        public int PercentComplete { get; set; }
        public int? IsRetake { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? DroppedDate { get; set; }
        public int EnrollmentStatusId { get; set; }
        public int? TermsOfServiceFlag { get; set; }
        public Int64? SectionStartTimestamp { get; set; }
        public int? CampaignId { get; set; }
        public int? CourseVariantId { get; set; }
        public virtual CampaignCourseVariant CampaignCourseVariant { get; set; }
        public int? Grade { get; set; }
    }
}