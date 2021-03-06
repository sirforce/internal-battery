﻿using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpDiddy.Api;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddy.Controllers
{
    [Authorize(Policy = "IsCareerCircleAdmin")]
    public class AdminController : Controller
    {
        private IApi _api;
        private IConfiguration _configuration;
        private IDistributedCache _cache;
        private IButterCMSService _butterService;


        public AdminController(IApi api, IConfiguration configuration, IDistributedCache cache, IButterCMSService butterService)
        {
            _api = api;
            _configuration = configuration;
            _cache = cache;
            _butterService = butterService;

        }


 
        [Authorize]
        [HttpGet]
        [Route("/admin/new-subscriber-csv")]
        public  async Task<FileResult> NewSubscriberCSV()
        {
            List<SubscriberInitialSourceDto> data = await _api.NewSubscribersCSVAsync();

            StringWriter csvString = new StringWriter();
            using (var csv = new CsvWriter(csvString))
            {
                csv.WriteRecords<SubscriberInitialSourceDto>(data);
            }

            string csvData = csvString.ToString();
            byte[] fileBytes = Encoding.ASCII.GetBytes(csvData);
            string fileName = "Subscribers_" + DateTime.UtcNow.ToString("yyyy -MM-dd-HH-mm-ss", CultureInfo.InvariantCulture) + ".csv";
            return File(fileBytes, "text/csv", fileName);  
        }

 

        [Authorize]
        [HttpGet]
        [Route("/admin/jobscrapestats")]
        public async Task<IActionResult> JobScrapeStats()
        {
            // get default number of records to view 
            int JobSiteScrapeAdminScreenDefaultNumRecords = int.Parse(_configuration["CareerCircle:JobSiteScrapeAdminScreenDefaultNumRecords"]);
            // check to see if it's overwritten by a query value 
            var queryNumRecords = Request.Query["NumRecords"];            
            if (queryNumRecords.ToString() != string.Empty)
                try
                {
                    JobSiteScrapeAdminScreenDefaultNumRecords = int.Parse(queryNumRecords);
                }
                catch { }
            IList<JobSiteScrapeStatisticDto> Statistics = await _api.JobScrapeStatisticsSearchAsync(JobSiteScrapeAdminScreenDefaultNumRecords);
            JobSiteScrapeStatisticViewModel model = new JobSiteScrapeStatisticViewModel()
            {
                Statistics = Statistics,
                NumRecords = JobSiteScrapeAdminScreenDefaultNumRecords
            };
            return View(model);
        }
        
        [HttpGet]
        [Route("/admin/courselookup")]
        public async Task<JsonResult> CourseLookup()
        {
            var selectListCourses = await _api.CoursesAsync();

            var list = selectListCourses
                .Select(course => new
                {
                    entityGuid = course.CourseGuid,
                    entityName = course.Name
                })
                .OrderBy(e => e.entityName)
                .ToList();

            return new JsonResult(list);
        }

        [HttpGet]
        [Route("/admin/subscriberlookup")]
        public async Task<JsonResult> SubscriberLookup()
        {
  
           ProfileSearchResultDto subs = await _api.SubscriberSearchAsync(string.Empty, string.Empty, string.Empty, string.Empty);

            var list = subs.Profiles
                .Select(subscriber => new
                {
                    entityGuid = subscriber.SubscriberGuid,
                    entityName = subscriber.Email
                })
                .OrderBy(e => e.entityName)
                .ToList();

            return new JsonResult(list);
        }

        [HttpGet]
        [Route("/admin/skillslookup/{entityType}/{entityGuid}")]
        public async Task<JsonResult> SkillsLookup(string entityType, Guid entityGuid)
        {
            var selectListSkills = await _api.GetEntitySkillsAsync(entityType, entityGuid);

            var list = selectListSkills
                .Select(skill => new
                {
                    skillGuid = skill.SkillGuid,
                    skillName = skill.SkillName
                })
                .OrderBy(s => s.skillName)
                .ToList();

            return new JsonResult(list);
        }

        [HttpGet]
        [Route("/admin/contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        [Route("/admin/skills")]
        public IActionResult Skills()
        {
            return View();
        }

        [HttpGet]
        [Route("/[controller]/dashboard")]
        public async Task<IActionResult> DashboardAsync()
        {
            List<DateTime> dates = new List<DateTime>();
            // get monday
            int delta = DayOfWeek.Monday - DateTime.Today.DayOfWeek;
            DateTime monday = DateTime.Today.AddDays(delta);
            for (int i = 0; i < 7; i++)
            {
                dates.Add(monday);
                monday = monday.AddDays(-7);
            }

            ViewBag.subscriberReport = await _api.GetSubscriberReportAsync(dates);
            ViewBag.partnerReport = await _api.GetSubscriberReportByPartnerAsync();
            ViewBag.offerActionSummary = await _api.GetOfferActionSummaryAsync();
            return View("Dashboard");
        }

        [HttpGet]
        [Route("/admin/subscriber-index-error")]
        public async Task<JsonResult> GetFailedSubscribers()
        {
            var data = await _api.GetFailedSubscribersSummaryAsync();
            return Json(data);
        }

        [HttpGet]
        [Route("/admin/partner-sub-action")]
        public async Task<JsonResult> GetPartnerSubActionReport()
        {
            var data = await _api.GetPartnerSubscriberActionStatsAsync();
            return Json(data);
        }

        [HttpGet]
        [Route("/admin/job-application-count")]
        public async Task<JsonResult> GetJobApplicationCount()
        {
            var data = await _api.GetJobApplicationCount();
            return Json(data);
        }

        [HttpPut]
        [Route("/admin/skills")]
        public async Task<IActionResult> UpdateSkills([FromBody] EntitySkillDto entitySkillDto)
        {
            // todo: exception handling
            await _api.UpdateEntitySkillsAsync(entitySkillDto);
            return Ok();
        }

        [HttpGet]
        [Route("/admin/contacts")]
        public IActionResult Contacts()
        {
            return View();
        }

        [HttpGet]
        [Route("/admin/partners/contacts/{PartnerGuid}")]
        public async Task<IActionResult> ContactsAsync(Guid PartnerGuid)
        {
            try
            {
                PartnerDto partner = await _api.GetPartnerAsync(PartnerGuid);

                if (partner == null)
                    return NotFound();


                return View("Contacts", partner);
            }
            catch (ApiException)
            {
                return BadRequest();
            }


        }

        [HttpPut]
        [Route("admin/contacts/import/{partnerGuid}/{cacheKey}")]
        public IActionResult ImportContacts(Guid partnerGuid, string cacheKey)
        {
            return new JsonResult(_api.ImportContactsAsync(partnerGuid, cacheKey));
        }

        [HttpPost]
        public IActionResult UploadContacts(IFormFile contactsFile)
        {
            ImportValidationSummaryDto importValidationSummary = new ImportValidationSummaryDto();
            try
            {
                var contacts = LoadContactsFromCsv(contactsFile);
                if (contacts != null)
                {
                    // perform basic validation on the contacts loaded from the csv file 
                    importValidationSummary.ImportActions = PerformBasicValidationOnContacts(ref contacts);
                    // load a handful of contact records for the UI preview
                    importValidationSummary.ContactsPreview = contacts.Take(5).ToList();
                    // store all contact data to be imported in redis (after removing items that will be skipped)
                    string cacheKey = StashContactForImportInRedis(contacts, Guid.Parse(this.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value));
                    // return the cache key to the UI so that the data can be retrieved later if processing continues
                    importValidationSummary.CacheKey = cacheKey;
                }
                else
                {
                    // no contacts could be loaded; indicate this in the import action
                    importValidationSummary.ImportActions.Add(new ImportActionDto()
                    {
                        Count = 0,
                        ImportBehavior = ImportBehavior.Pending,
                        Reason = "No records could be loaded from the selected file(s)"
                    });
                }
            }
            catch (Exception e)
            {
                importValidationSummary.ErrorMessage = e.Message;
            }
            return new JsonResult(importValidationSummary);
        }

        /// <summary>
        /// Performs basic validation on the list of contacts uploaded (required fields, duplicates by email)
        /// </summary>
        /// <param name="contacts">The list of contacts to be acted upon; note that this is passed by referenced and will be modified if any contacts fail validation</param>
        /// <returns></returns>
        private List<ImportActionDto> PerformBasicValidationOnContacts(ref List<ContactDto> contacts)
        {
            List<ImportActionDto> importActions = new List<ImportActionDto>();
            int contactsToBeProcessed = contacts.Count();

            // email check
            var missingEmail = contacts.Where(c => string.IsNullOrWhiteSpace(c.Email)).ToList();
            if (missingEmail != null && missingEmail.Count() > 1)
            {
                importActions.Add(
                    new ImportActionDto()
                    {
                        Count = missingEmail.Count(),
                        ImportBehavior = ImportBehavior.Ignored,
                        Reason = "missing required field: Email"
                    });
                contactsToBeProcessed -= contacts.RemoveAll(c => string.IsNullOrWhiteSpace(c.Email));
            }

            // metadata check
            var missingMetadataRequiredFields = contacts.Where(c => !c.Metadata.ContainsKey("FirstName") || c.Metadata.ContainsKeyValue("FirstName", string.Empty) || !c.Metadata.ContainsKey("LastName") || c.Metadata.ContainsKeyValue("LastName", string.Empty)).ToList();
            if (missingMetadataRequiredFields != null && missingMetadataRequiredFields.Count() > 1)
            {
                importActions.Add(
                    new ImportActionDto()
                    {
                        Count = missingMetadataRequiredFields.Count(),
                        ImportBehavior = ImportBehavior.Ignored,
                        Reason = "missing required field(s): FirstName, LastName"
                    });
                contactsToBeProcessed -= contacts.RemoveAll(c => !c.Metadata.ContainsKey("FirstName") || c.Metadata.ContainsKeyValue("FirstName", string.Empty) || !c.Metadata.ContainsKey("LastName") || c.Metadata.ContainsKeyValue("LastName", string.Empty));
            }

            // duplicate check
            var duplicates = (from c in contacts
                              group c by new { c.Email } into g
                              where g.Count() > 1
                              select new
                              {
                                  Email = g.First().Email,
                                  Instances = g.Count(),
                                  SourceSystemIdentifier = g.First().SourceSystemIdentifier,
                                  Metadata = g.First().Metadata
                              }).ToList();
            if (duplicates != null && duplicates.Count() > 1)
            {
                importActions.Add(
                    new ImportActionDto()
                    {
                        Count = duplicates.Sum(d => d.Instances) - duplicates.Count,
                        ImportBehavior = ImportBehavior.Ignored,
                        Reason = "identified as duplicates"
                    });
                contactsToBeProcessed -= contacts.RemoveAll(c => duplicates.Select(d => d.Email).Contains(c.Email));
            }

            // remaining records to be processed
            importActions.Add(
                new ImportActionDto()
                {
                    Count = contactsToBeProcessed,
                    ImportBehavior = ImportBehavior.Pending,
                    Reason = "passed validation rules"
                });

            return importActions;
        }

        private List<ContactDto> LoadContactsFromCsv(IFormFile contactsFile)
        {
            List<ContactDto> contacts;
            using (var reader = new StreamReader(contactsFile.OpenReadStream()))
            {
                using (var csv = new CsvReader(reader))
                {
                    // do this outside of ContactDtoMap for performance reasons
                    List<string> declaredProperties = typeof(ContactDto)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Select(p => p.Name.ToLower())
                        .ToList();

                    ContactDtoMap contactDtoMap = new ContactDtoMap(declaredProperties);
                    csv.Configuration.RegisterClassMap(contactDtoMap);
                    csv.Configuration.Delimiter = ",";
                    csv.Configuration.PrepareHeaderForMatch = (string header, int index) =>
                    {
                        var newHeader = Regex.Replace(header, @"\s", string.Empty);
                        newHeader = newHeader.Trim();
                        newHeader = newHeader.ToLower();
                        return newHeader;
                    };

                    contacts = csv.GetRecords<ContactDto>().ToList();
                }
            }
            return contacts;
        }

        private string StashContactForImportInRedis(List<ContactDto> contacts, Guid subscriberGuid)
        {
            int cacheTtl = int.Parse(_configuration["redis:cacheTTLInMinutes"]);
            string contactsJson = Newtonsoft.Json.JsonConvert.SerializeObject(contacts);
            string cacheKey = $"contactImport:{Guid.NewGuid()}";
            _cache.SetString(cacheKey, contactsJson, new DistributedCacheEntryOptions() { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(cacheTtl) });
            return cacheKey;
        }

        public sealed class ContactDtoMap : ClassMap<ContactDto>
        {
            public ContactDtoMap(List<string> declaredProperties)
            {
                Map(c => c.Email).Name("email");
                Map(c => c.SourceSystemIdentifier).Name("sourcesystemidentifier");
                Map(c => c.Metadata).ConvertUsing(row =>
                {
                    Dictionary<string, string> metadata = new Dictionary<string, string>();
                    var metadataColumnNames = row.Context.HeaderRecord.ToList().Except(declaredProperties).ToList();
                    if (metadataColumnNames != null && metadataColumnNames.Count > 0)
                    {
                        foreach (var metaDataColumnName in metadataColumnNames)
                        {
                            string fieldValue = null;
                            if (row.TryGetField<string>(metaDataColumnName, out fieldValue))
                            {
                                metadata.Add(metaDataColumnName, fieldValue);
                            }
                        }
                    }

                    return metadata;
                });
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Partners()
        {
            return View();
        }

        [Authorize]
        [HttpGet("/admin/modifypartner/{PartnerGuid}")]
        public async Task<IActionResult> ModifyPartnerAsync(Guid PartnerGuid)
        {
            PartnerDto partner = await _api.GetPartnerAsync(PartnerGuid);

            if (partner == null)
                return NotFound();

            return View("ModifyPartner", partner);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePartnerAsync(PartnersViewModel UpdatedPartner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    BasicResponseDto NewPartnerResponse = await _api.UpdatePartnerAsync(new PartnerDto
                    {
                        PartnerGuid = UpdatedPartner.PartnerGuid,
                        Name = UpdatedPartner.Name,
                        Description = UpdatedPartner.Description
                    });
                    return RedirectToAction("Partners");
                }
                catch (ApiException)
                {
                    // Log exception
                }
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult> PartnerGridAsync(String searchQuery)
        {
            IList<PartnerDto> partners = await _api.GetPartnersAsync();
            return PartialView("Admin/_PartnerGrid", partners);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddPartner()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePartnerAsync(PartnersViewModel NewPartner)
        {


            if (ModelState.IsValid)
            {
                try
                {
                    PartnerDto newPartnerFromDb = await _api.CreatePartnerAsync(new PartnerDto
                    {
                        PartnerGuid = NewPartner.PartnerGuid,
                        Name = NewPartner.Name,
                        Description = NewPartner.Description
                    });
                    return RedirectToAction("Partners");
                }
                catch (ApiException)
                {
                    // Log error
                }
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet("/admin/deletepartner/{PartnerGuid}")]
        public async Task<IActionResult> DeleteParterAsync(Guid PartnerGuid)
        {
            try
            {
                BasicResponseDto response = await _api.DeletePartnerAsync(PartnerGuid);
                if (response.StatusCode != 200)
                    return BadRequest();
                return RedirectToAction("Partners");
            }
            catch (ApiException)
            {
                // Log error
            }
            return BadRequest();
        }


        [Authorize]
        [HttpGet]
        public IActionResult Notifications()
        {
            return View();
        }

        [Authorize]
        [HttpGet("/admin/modifynotification/{NotificationGuid}")]
        public async Task<IActionResult> ModifyNotificationAsync(Guid NotificationGuid)
        {
            NotificationDto notification = await _api.GetNotificationAsync(NotificationGuid);

            if (notification == null)
                return NotFound();

            return View("ModifyNotification", notification);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateNotificationAsync(NotificationsViewModel UpdatedNotification)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    BasicResponseDto NewNotificationResponse = await _api.UpdateNotificationAsync(new NotificationDto
                    {
                        NotificationGuid = UpdatedNotification.NotificationGuid,
                        Title = UpdatedNotification.Title,
                        Description = UpdatedNotification.Description,
                        ExpirationDate = UpdatedNotification.ExpirationDate
                    });
                    return RedirectToAction("Notifications");
                }
                catch (ApiException)
                {
                    // Log exception
                }
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        public async Task<PartialViewResult> NotificationGridAsync(String searchQuery)
        {
            IList<NotificationDto> notifications = await _api.GetNotificationsAsync();
            return PartialView("Admin/_NotificationGrid", notifications);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddNotification()
        {
            List<GroupDto> Groups = await _api.GetGroupsAsync();
            return View(new NotificationsViewModel { Groups = Groups});
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateNotificationAsync(NotificationsViewModel NewNotification)
        {
 

            if (ModelState.IsValid)
            {
                try
                {
                    NewNotificationDto newNotificationFromDb = await _api.CreateNotificationAsync(new NewNotificationDto
                    {
                        NotificationDto = new NotificationDto{
                            NotificationGuid = Guid.NewGuid(),
                            Title = NewNotification.Title,
                            Description = NewNotification.Description,
                            ExpirationDate = NewNotification.ExpirationDate,
                            IsTargeted = NewNotification.IsTargeted
                        },
                        GroupGuid = NewNotification.GroupGuid

                    });
                    return RedirectToAction("Notifications");
                }
                catch (ApiException)
                {
                    // Log error
                }
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet("/admin/deletenotification/{NotificationGuid}")]
        public async Task<IActionResult> DeleteNotificationAsync(Guid NotificationGuid)
        {
            try
            {
                BasicResponseDto response = await _api.DeleteNotificationAsync(NotificationGuid);
                if (response.StatusCode != 200)
                    return BadRequest();
                return RedirectToAction("Notifications");
            }
            catch (ApiException)
            {
                // Log error
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet("/admin/clearcmscache")]
        public IActionResult ClearCmsCache(){
            ViewBag.Environment = _configuration["redis:name"];
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ClearCmsCacheAsync([FromBody] CachedPageDto CachedPage)
        {
            if(!CachedPage.Slug.ToLower().Contains(_configuration["Environment:BaseUrl"].Substring(0, _configuration["Environment:BaseUrl"].Length - 1).ToLower()))
                return NotFound();

            bool ClearedCache = await _butterService.ClearCachedPageAsync(CachedPage.Slug);

            if(ClearedCache)
                return Ok();
            else
                return NotFound();
        }

        #region Company
        [Authorize]
        [HttpGet("/[controller]/companies")]
        public IActionResult GetCompanies()
        {
            return View("Companies");
        }
        #endregion

        #region Courses
        [Authorize]
        [HttpGet("/[controller]/coursesites")]
        public IActionResult GetCourseSites()
        {
            return View("CourseSites");
        }
        #endregion

        #region Recruiters
        [Authorize]
        [HttpGet("/[controller]/recruiters")]
        public IActionResult GetRecruiters()
        {
            return View("Recruiters");
        }
        #endregion
    }
}