﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using UpDiddyApi.ApplicationCore.Services.Identity;
using UpDiddyApi.ApplicationCore.Services.Identity.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyLib.Dto.User;
using UpDiddyLib.Dto;
using UpDiddyApi.ApplicationCore.Interfaces;

namespace UpDiddyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ISubscriberService _subscriberService;
        private readonly IJobService _jobService;
        private readonly IB2CGraph _graphClient;
        private readonly IAPIGateway _adb2cApi;

        public IdentityController(IServiceProvider services)
        {
            _mapper = services.GetService<IMapper>();
            _userService = services.GetService<IUserService>();
            _subscriberService = services.GetService<ISubscriberService>();
            _jobService = services.GetService<IJobService>();
            _graphClient = services.GetService<IB2CGraph>();
            _adb2cApi = services.GetService<IAPIGateway>();
        }

        [HttpGet("check-auth0/{email}")]
        public async Task<IActionResult> IsUserInAuth0(string email)
        {
            // todo: secure this request using a secret stored in the key vault 
            if (false)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _userService.GetUserByEmailAsync(email);

            if (result != null && result.Success)
            {
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = "A user with that email exists in Auth0." });
            }
            else
            {
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "A user with that email does not exist in Auth0." });
            }
        }

        [HttpGet("check-adb2c/{email}")]
        public async Task<IActionResult> IsUserInADB2C(string email)
        {
            // todo: secure this request using a secret stored in the key vault 
            if (false)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // check if user exits in ADB2C (the account must also be enabled)
            Microsoft.Graph.User user = await _graphClient.GetUserBySignInEmail(email);

            if (user != null && user.AccountEnabled.HasValue && user.AccountEnabled.Value)
            {
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = "A user with that email exists in ADB2C." });
            }
            else
            {
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "A user with that email does not exist in ADB2C." });
            }
        }

        [HttpPost("check-adb2c-login")]
        public async Task<IActionResult> CheckADB2CLogin(UserDto userDto)
        {
            // todo: secure this request using a secret stored in the key vault 
            if (false)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _adb2cApi.CheckADB2CLogin(userDto.Email, userDto.Password);

            if (result)
            {
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = "This login is valid for ADB2C." });
            }
            else
            {
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "This login is not valid for ADB2C." });
            }
        }

        [HttpPost("migrate-user")]
        public async Task<IActionResult> MigrateUserAsync([FromBody] CreateUserDto createUserDto)
        {    
            // todo: secure this request using a secret stored in the key vault 
            if (false)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<CreateUserDto, User>(createUserDto);

            var result = await _userService.MigrateUserAsync(user);

            if (result != null && result.Success)
            {
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = "The user has been migrated successfully." });
            }
            else
            {
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "Tue user was not migrated successfully." });
            }
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto createUserDto)
        {
            // todo: secure this request using a secret stored in the key vault 
            if (false)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<CreateUserDto, User>(createUserDto);

            // the publicly available endpoint should not allow user creation without email verification nor does it allow special roles to be assigned
            var createLoginResponse = await _userService.CreateUserAsync(user, true, null);

            if (!createLoginResponse.Success)
            {
                return Ok(new BasicResponseDto() { StatusCode = 404, Description = createLoginResponse.Message });
            }
            else
            {
                createUserDto.SubscriberGuid = createLoginResponse.User.SubscriberGuid;
                var createSubscriberResult = await _subscriberService.CreateSubscriberAsync(createUserDto);
                // if the subscriber is not created successfully, remove the associated login that was created and return a failure message
                if (!createSubscriberResult)
                {
                    _userService.DeleteUserAsync(user.UserId);
                    return Ok(new BasicResponseDto() { StatusCode = 404, Description = "An error occurred creating the user." });
                }
                else
                {
                    // Store the job referral code if one was supplied
                    if (createUserDto.JobReferralCode != null)
                    {
                        await _jobService.UpdateJobReferral(createUserDto.JobReferralCode, createUserDto.SubscriberGuid.ToString());
                    }
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = createLoginResponse.Message });
                }
            }
        }
    }
}