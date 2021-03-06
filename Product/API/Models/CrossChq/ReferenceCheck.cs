﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.Models.G2;

namespace UpDiddyApi.Models.CrossChq
{
    [Table("ReferenceCheck", Schema = "G2")]
    public class ReferenceCheck : BaseModel
    {
        public int ReferenceCheckId { get; set; }

        public Guid ReferenceCheckGuid { get; set; }

        /// <summary>
        /// The request id tied to the vendor-webapi response when a request is made.
        /// </summary>
        [StringLength(150, MinimumLength = 1)]
        public string ReferenceCheckRequestId { get; set; }

        /// <summary>
        /// The UTC datetime of the reference check when concluded.
        /// If null the reference check is incomplete.
        /// </summary>
        public DateTime? ReferenceCheckConcludedDate { get; set; }

        /// <summary>
        /// Type of reference check type, for example, the job_role json property that is returned by CrossChq API response.
        /// </summary>
        [StringLength(100, MinimumLength = 1)]
        public string ReferenceCheckType { get; set; }

        [Required]
        [StringLength(75, MinimumLength = 1)]
        public string CandidateJobTitle { get; set; }

        /// <summary>
        /// Candidate's <see cref="Profile.ProfileId">ProfileId<>.
        /// </summary>
        [Required]
        public int ProfileId { get; set; }
        public virtual Profile Profile { get; set; }

        [Required]
        public int ReferenceCheckVendorId { get; set; }
        public virtual ReferenceCheckVendor ReferenceCheckVendor { get; set; }

        [Required]
        public int RecruiterId { get; set; }
        public virtual Recruiter Recruiter { get; set; }

        public virtual List<CandidateReference> CandidateReference { get; set; } = new List<CandidateReference>();

        public virtual List<ReferenceCheckStatus> ReferenceCheckStatus { get; set; } = new List<ReferenceCheckStatus>();

        public virtual List<ReferenceCheckReport> ReferenceCheckReport { get; set; } = new List<ReferenceCheckReport>();


    }
}
