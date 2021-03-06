﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddyLib.Domain.Models;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
namespace UpDiddyApi.Controllers
{
    [Route("/V2/[controller]/")]
    public class SkillsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHangfireService _hangfireService;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ISkillService _skillservice;

        public SkillsController(IMapper mapper
        , IConfiguration configuration
        , IHangfireService hangfireService
        , ISkillService skillService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _skillservice = skillService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSkills(int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            var skills = await _skillservice.GetSkills(limit, offset, sort, order);
            return Ok(skills);
        }

        [HttpGet]
        [Route("keyword")]
        public async Task<IActionResult> GetSkillsByKeyword([FromQuery] string value)
        {
            var skills = await _skillservice.GetSkillsByKeyword(value);
            return Ok(skills);
        }

        [HttpGet]
        [Route("{skill:guid}")]
        public async Task<IActionResult> GetSkill(Guid skill)
        {
            var result = await _skillservice.GetSkill(skill);
            return Ok(result);
        }

        [HttpPut]
        [Route("{skill:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateSkill(Guid skill, [FromBody] SkillDto skillDto)
        {
            await _skillservice.UpdateSkill(skill, skillDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{skill:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteSkill(Guid skill)
        {
            await _skillservice.DeleteSkill(skill);
            return StatusCode(204);
        }

        [HttpPost]
        [Route("{skill:guid}")]
        [Authorize]
        public async Task<IActionResult> CreateSkill(Guid skill, [FromBody] SkillDto skillDto)
        {
            var skillGuid = await _skillservice.CreateSkill(skillDto);
            return StatusCode(201, skillGuid);
        }

        [Obsolete("Remove this once the React app is using the route in the CoursesController.", false)]
        [HttpGet]
        [Route("courses/{course:guid}")]
        public async Task<IActionResult> GetSkillForCourse(Guid course)
        {
            var result = await _skillservice.GetSkillsByCourseGuid(course);
            return Ok(result);
        }
    }
}