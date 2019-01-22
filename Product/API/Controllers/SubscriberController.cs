﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddyApi.Authorization;
using UpDiddyApi.Business.Graph;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;
using System.IO;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriberController : Controller
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger _syslog;
        private IB2CGraph _graphClient;
        private ICloudStorage _cloudStorage;

        public SubscriberController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ILogger<SubscriberController> sysLog, IDistributedCache distributedCache, IB2CGraph client, ICloudStorage cloudStorage)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _syslog = sysLog;
            _graphClient = client;
            _cloudStorage = cloudStorage;
        }

        [HttpGet("/api/[controller]/me/group")]
        public async Task<IActionResult> MyGroupsAsync()
        {
            IList<Microsoft.Graph.Group> groups = await _graphClient.GetUserGroupsByObjectId(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            IList<string> response = new List<string>();

            foreach (var group in groups)
            {
                ConfigADGroup acceptedGroup = _configuration.GetSection("ADGroups:Values")
                    .Get<List<ConfigADGroup>>()
                    .Find(e => e.Id == group.Id);

                if (acceptedGroup != null)
                    response.Add(acceptedGroup.Name);
            }

            return Json(new { groups = response });
        }

        [HttpGet("/api/[controller]/search/{searchQuery}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public IActionResult Search(string searchQuery)
        {
            List<Subscriber> subscribers = _db.Subscriber
                .Include(s => s.SubscriberSkills)
                .ThenInclude(s => s.Skill)
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .Where(s => s.IsDeleted == 0
                && (s.Email.Contains(searchQuery)
                    || s.FirstName.Contains(searchQuery)
                    || s.LastName.Contains(searchQuery)
                    || s.PhoneNumber.Contains(searchQuery)
                    || s.Address.Contains(searchQuery)
                    || s.City.Contains(searchQuery)
                    || s.SubscriberSkills.Where(k => k.Skill.SkillName.Contains(searchQuery)).Any()
                    || s.SubscriberWorkHistory.Where(w => w.JobDecription.Contains(searchQuery)).Any()
                    || s.SubscriberWorkHistory.Where(w => w.Title.Contains(searchQuery)).Any()
                    || s.SubscriberWorkHistory.Where(w => w.Company.CompanyName.Contains(searchQuery)).Any()
                    || s.SubscriberEducationHistory.Where(e => e.EducationalDegree.Degree.Contains(searchQuery)).Any()
                    || s.SubscriberEducationHistory.Where(e => e.EducationalDegreeType.DegreeType.Contains(searchQuery)).Any()
                    || s.SubscriberEducationHistory.Where(e => e.EducationalInstitution.Name.Contains(searchQuery)).Any())
                )
                .ToList();

            return Json(_mapper.Map<List<SubscriberDto>>(subscribers));

            return View();
        }
        
        [HttpGet("/api/[controller]/search")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public IActionResult Search()
        {
            List<Subscriber> subscribers = _db.Subscriber
                .Where(s => s.IsDeleted == 0)
                .Include(s => s.SubscriberSkills)
                .ThenInclude(s => s.Skill)
                .Include(s => s.State)
                .ThenInclude(s => s.Country)
                .ToList();

            return Json(_mapper.Map<List<SubscriberDto>>(subscribers));
        }

        [HttpGet("{subscriberGuid}")]
        [Authorize(Policy = "IsRecruiterPolicy")]
        public IActionResult Get(Guid subscriberGuid)
        {
            Subscriber subscriber = _db.Subscriber
                .Where(s => s.IsDeleted == 0 && s.SubscriberGuid == subscriberGuid)
                .Include(s => s.State).ThenInclude(c => c.Country)
                .Include(s => s.SubscriberSkills).ThenInclude(ss => ss.Skill)
                .Include(s => s.Enrollments).ThenInclude(e => e.Course)
                .Include(s => s.SubscriberWorkHistory).ThenInclude(swh => swh.Company)
                .Include(s => s.SubscriberWorkHistory).ThenInclude(swh => swh.CompensationType)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalInstitution)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalDegreeType)
                .Include(s => s.SubscriberEducationHistory).ThenInclude(seh => seh.EducationalDegree)
                .FirstOrDefault();

            if (subscriber == null)
                return NotFound();
            else
                return Ok(_mapper.Map<SubscriberDto>(subscriber));
        }

        [Authorize]
        [HttpGet("/api/[controller]/{subscriberGuid}/file/{fileId}")]
        public async Task<IActionResult> DownloadFile(Guid subscriberGuid, int fileId)
        {
            Guid userGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userGuid != subscriberGuid)
                return Unauthorized();

            Subscriber subscriber = _db.Subscriber.Where(s => s.SubscriberGuid.Equals(subscriberGuid))
                .Include(s => s.SubscriberFile)
                .First();
            SubscriberFile file = subscriber.SubscriberFile.Where(f => f.Id == fileId).First();
            return File(await _cloudStorage.OpenReadAsync(file.BlobName), "application/octet-stream", Path.GetFileName(file.BlobName));
        }

        [Authorize]
        [HttpDelete("/api/[controller]/{subscriberGuid}/file/{fileId}")]
        public async Task<IActionResult> DeleteFile(Guid subscriberGuid, int fileId)
        {
            Guid userGuid = Guid.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userGuid != subscriberGuid)
                return Unauthorized();

            Subscriber subscriber = _db.Subscriber.Where(s => s.SubscriberGuid.Equals(subscriberGuid))
                .Include(s => s.SubscriberFile)
                .First();
            SubscriberFile file = subscriber.SubscriberFile.Where(f => f.Id == fileId).First();

            if (!await _cloudStorage.DeleteFileAsync(file.BlobName))
                return BadRequest();

            _db.SubscriberFile.Remove(file);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
