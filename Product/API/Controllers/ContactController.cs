﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.MessageQueue;
using Microsoft.EntityFrameworkCore;
using UpDiddyApi.ApplicationCore;
using UpDiddyLib.Helpers;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hangfire;
using UpDiddyApi.Workflow;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly UpDiddyDbContext _db = null;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly string _queueConnection = string.Empty;
        protected readonly ILogger _syslog = null;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly ISysEmail _sysemail;
        private readonly IDistributedCache _distributedCache;

        public ContactController(UpDiddyDbContext db, IMapper mapper, IConfiguration configuration, ISysEmail sysemail, IHttpClientFactory httpClientFactory, ILogger<CourseController> syslog, IDistributedCache distributedCache)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _queueConnection = _configuration["CareerCircleQueueConnection"];
            _syslog = syslog;
            _httpClientFactory = httpClientFactory;
            _sysemail = sysemail;
            _distributedCache = distributedCache;
        }

        [HttpGet("{contactGuid}")]
        public IActionResult Get(Guid ContactGuid)
        {
            ContactDto rval = null;
            rval = _db.Contact
                .Where(t => t.IsDeleted == 0 && t.ContactGuid == ContactGuid)
                .ProjectTo<ContactDto>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
            if (rval == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(rval);
            }
        }


        [HttpPut]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        [Route("api/[controller]/import/{partnerGuid}/{cacheKey}")]
        public IActionResult ImportContacts(Guid? partnerGuid, string cacheKey, [FromBody] List<ContactDto> contacts)
        {
            if (!partnerGuid.HasValue || partnerGuid.Value == Guid.Empty || cacheKey == null)
                return BadRequest();

            // todo: existing contacts

            var newContacts =
                from db in _db.Contact
                join upload in contacts on db.Email equals upload.Email into temp
                from upload in temp.DefaultIfEmpty()
                select new
                {
                    db.ContactId,
                    Email = upload.Email,
                    SourceSystemIdentifier = upload.SourceSystemIdentifier,
                    Metadata = upload.Metadata
                };

            // todo: can we yield return records as they are processed? do we want to process them one at a time?

            return Ok();
        }
    }
}
