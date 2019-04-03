﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.Models;
using UpDiddyLib.Dto.Reporting;

namespace UpDiddyApi.Controllers
{
    [Authorize(Policy = "IsCareerCircleAdmin")]
    public class ReportController : Controller
    {
        private UpDiddyDbContext _db { get; set; }

        public ReportController(UpDiddyDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("/api/[controller]/offer-action-summary")]
        public async Task<IActionResult> OfferActionSummary()
        {
            return Ok(
                _db.SubscriberOfferActions
                .GroupBy(rca => new { rca.Action, rca.OfferName, rca.OfferCode })
                .Select(g => new OfferActionSummaryDto()
                {
                    Action = g.Key.Action,
                    ActionCount = g.Count(),
                    OfferName = g.Key.OfferName,
                    OfferCode = g.Key.OfferCode
                })
                .OrderByDescending(r => r.ActionCount)
                .ToList());
        }

        [HttpGet]
        [Route("/api/[controller]/recruiter-action-summary")]
        public async Task<IActionResult> RecruiterActionSummary()
        {
            return Ok(
                _db.RecruiterSubscriberActions
                .GroupBy(rca => new { rca.RecruiterEmail, rca.RecruiterFirstName, rca.RecruiterLastName, rca.Action })
                .Select(g => new RecruiterActionSummaryDto()
                {
                    Action = g.Key.Action,
                    ActionCount = g.Count(),
                    RecruiterEmail = g.Key.RecruiterEmail,
                    RecruiterFirstName = g.Key.RecruiterFirstName,
                    RecruiterLastName = g.Key.RecruiterLastName,
                })
                .OrderByDescending(r => r.ActionCount)
                .ToList());
        }

        [HttpGet]
        [Route("/api/[controller]/subscriber-action-summary")]
        public async Task<IActionResult> SubscriberActionSummary()
        {
            return Ok(
                _db.RecruiterSubscriberActions
                .GroupBy(rca => new { rca.SubscriberEmail, rca.SubscriberFirstName, rca.SubscriberLastName, rca.Action })
                .Select(g => new SubscriberActionSummaryDto()
                {
                    Action = g.Key.Action,
                    ActionCount = g.Count(),
                    SubscriberEmail = g.Key.SubscriberEmail,
                    SubscriberFirstName = g.Key.SubscriberFirstName,
                    SubscriberLastName = g.Key.SubscriberLastName,
                })
                .OrderByDescending(r => r.ActionCount)
                .ToList());
        }

        [HttpGet]
        [Route("/api/[controller]/subscribers")]
        public async Task<IActionResult> SubscribersReport([FromQuery] List<DateTime> dates)
        {
            List<BasicCountReportDto> totalsByDate = new List<BasicCountReportDto>();
            var subscriberQuery = _db.Subscriber.AsQueryable();
            var enrollmentQuery = _db.Enrollment.AsQueryable();

            BasicCountReportDto totals = new BasicCountReportDto()
            {
                SubscriberCount = subscriberQuery.Count(),
                EnrollmentCount = enrollmentQuery.Count()
            };

            if (!dates.Any())
                return Ok(new SubscriberReportDto()
                {
                    Totals = totals
                });

            dates.Sort();
            DateTime? prevDate = null;
            for (int i = dates.Count - 1; i >= 0; i--)
            {
                BasicCountReportDto bcr = new BasicCountReportDto();
                DateTime startDate = dates[i];
                subscriberQuery = _db.Subscriber.Where(s => s.CreateDate >= startDate);
                enrollmentQuery = _db.Enrollment.Where(s => s.DateEnrolled >= startDate);

                if (prevDate.HasValue)
                {
                    subscriberQuery = subscriberQuery.Where(s => s.CreateDate < prevDate);
                    enrollmentQuery = enrollmentQuery.Where(s => s.DateEnrolled < prevDate);

                    bcr.EndDate = prevDate.Value;
                }

                bcr.StartDate = startDate;
                bcr.SubscriberCount = subscriberQuery.Count();
                bcr.EnrollmentCount = enrollmentQuery.Count();
                totalsByDate.Add(bcr);

                prevDate = startDate;
            }

            return Ok(new SubscriberReportDto()
            {
                Totals = totals,
                Report = totalsByDate
            });
        }


        [HttpGet]
        [Route("/api/[controller]/partners")]
        public async Task<IActionResult> SubscriberReportByPartner([FromQuery] List<DateTime> dates)
        {
            var query = from s in _db.SubscriberSignUpPartnerReferences
                        join sub in _db.Subscriber on s.SubscriberId equals sub.SubscriberId
                        join p in _db.Partner on s.PartnerId equals p.PartnerId into pGroup
                        from partner in pGroup.DefaultIfEmpty()
                        join e in _db.Enrollment on s.SubscriberId equals e.SubscriberId into eGroup
                        from enrollment in eGroup.DefaultIfEmpty()
                        group new
                        {
                            PartnerName = partner == null ? "N/A" : partner.Name,
                            HasEnrollment = enrollment != null,
                            SubscriberId = s.SubscriberId
                        }
                        by (partner.PartnerId == null) ? -1 : partner.PartnerId into report
                        select new
                        {
                            subscriberCount = report.Select(x => x.SubscriberId).Distinct().Count(),
                            enrollmentCount = report.Count(x => x.HasEnrollment),
                            partnerName = report.First().PartnerName
                        };
            return Ok(new { report = query.ToList() });
        }
    }
}