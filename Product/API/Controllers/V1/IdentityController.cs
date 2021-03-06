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
using UpDiddyApi.Authorization;
using Microsoft.Extensions.Logging;

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
        private readonly IPasswordResetRequestService _passwordResetRequestService;
        private ILogger _syslog = null;
        public IdentityController(IServiceProvider services, ILogger<IdentityController> sysLog)
        {
            _mapper = services.GetService<IMapper>();
            _userService = services.GetService<IUserService>();
            _subscriberService = services.GetService<ISubscriberService>();
            _jobService = services.GetService<IJobService>();
            _graphClient = services.GetService<IB2CGraph>();
            _adb2cApi = services.GetService<IAPIGateway>();
            _passwordResetRequestService = services.GetService<IPasswordResetRequestService>();
            _syslog = sysLog;
        }

        // intentionally using HttpPost rather than HttpGet because of how IIS treats certain special characters in routes even if they are escaped (such as +)
        // https://stackoverflow.com/questions/7739233/double-escape-sequence-inside-a-url-the-request-filtering-module-is-configured
        [HttpPost("is-user-exists-auth0")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> IsUserInAuth0([FromBody] EmailDto emailDto)
        {
            var result = await _userService.GetUserByEmailAsync(emailDto.Email);

            if (result != null && result.Success)
            {
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = "A user with that email exists in Auth0." });
            }
            else
            {
                _syslog.LogError($"IdentityController:IsUserInAuth0  A user with that email does not exist in Auth0.");
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "A user with that email does not exist in Auth0." });
            }
        }

        // intentionally using HttpPost rather than HttpGet because of how IIS treats certain special characters in routes even if they are escaped (such as +)
        // https://stackoverflow.com/questions/7739233/double-escape-sequence-inside-a-url-the-request-filtering-module-is-configured
        [HttpPost("is-user-exists-adb2c")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> IsUserInADB2C([FromBody] EmailDto emailDto)
        {
            // check if user exits in ADB2C (the account must also be enabled)
            Microsoft.Graph.User user = await _graphClient.GetUserBySignInEmail(emailDto.Email);

            if (user != null && user.AccountEnabled.HasValue && user.AccountEnabled.Value)
            {
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = "A user with that email exists in ADB2C." });
            }
            else
            {
                _syslog.LogError($"IdentityController:IsUserInADB2C  A user with that email does not exist in ADB2C.");
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "A user with that email does not exist in ADB2C." });
            }
        }

        [HttpPost("check-adb2c-login")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> CheckADB2CLogin([FromBody] UserDto userDto)
        {
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
                _syslog.LogError($"IdentityController:CheckADB2CLogin This login is not valid for ADB2C.");
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "This login is not valid for ADB2C." });
            }
        }

        [HttpPost("migrate-user")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> MigrateUserAsync([FromBody] CreateUserDto createUserDto)
        {
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
                _syslog.LogError($"IdentityController:MigrateUserAsync The user was not migrated successfully.");
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "The user was not migrated successfully." });
            }
        }

        [HttpPost("create-custom-password-reset")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> CreateCustomPasswordResetAsync([FromBody] EmailDto emailDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subscriber = await _subscriberService.GetSubscriberByEmail(emailDto.Email);

            if (subscriber == null || !subscriber.SubscriberGuid.HasValue)
            {
                _syslog.LogError($"IdentityController:CreateCustomPasswordResetAsync Subscriber does not exist.");
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "Subscriber does not exist." });
            }
                

            try
            {
                await _passwordResetRequestService.CreatePasswordResetRequest(subscriber.SubscriberGuid.Value);
            }
            catch (Exception e)
            {
                return Ok(new BasicResponseDto() { StatusCode = 500, Description = e.Message });
            }

            return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Password reset request was initiated successfully." });
        }

        [HttpPost("consume-custom-password-reset")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> ConsumeCustomPasswordResetAsync([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool isPasswordResetRequestSuccessful = await _passwordResetRequestService.ConsumePasswordResetRequest(resetPasswordDto);

            if (isPasswordResetRequestSuccessful)
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Password reset completed successfully." });
            else
            {
                _syslog.LogError($"IdentityController:ConsumeCustomPasswordResetAsync Password reset was not completed successfully.");
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "Password reset was not completed successfully." });
            }
                
        }

        [HttpGet("check-custom-password-reset/{passwordResetRequestGuid}")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> CheckValidityOfPasswordResetRequest(Guid passwordResetRequestGuid)
        {
            if(passwordResetRequestGuid == null || passwordResetRequestGuid == Guid.Empty)
            {
                _syslog.LogError($"IdentityController:CheckValidityOfPasswordResetRequest Password reset request is not valid. Missing Guid");
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "Password reset request is not valid." });
            }
                

            bool isPasswordResetRequestValid = await _passwordResetRequestService.CheckValidityOfPasswordResetRequest(passwordResetRequestGuid);


            if (isPasswordResetRequestValid)
                return Ok(new BasicResponseDto() { StatusCode = 200, Description = "Password reset request is valid." });
            else
            {
                _syslog.LogError($"IdentityController:CheckValidityOfPasswordResetRequest Password reset request is not valid. Operation failed.");
                return Ok(new BasicResponseDto() { StatusCode = 400, Description = "Password reset request is not valid." });
            }
                
        }
        

        
        [HttpPost("create-user")]
        [MiddlewareFilter(typeof(UserManagementAuthorizationPipeline))]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<CreateUserDto, User>(createUserDto);

            // the publicly available endpoint should not allow user creation without email verification nor does it allow special roles to be assigned
            var createLoginResponse = await _userService.CreateUserAsync(user, true, null);

            if (!createLoginResponse.Success)
            {
                _syslog.LogError($"IdentityController:CreateUserAsync {createLoginResponse.Message} for user {createUserDto.Email}");
                return Ok(new BasicResponseDto() { StatusCode = 404, Description = createLoginResponse.Message });
            }
            else
            {
                createUserDto.SubscriberGuid = createLoginResponse.User.SubscriberGuid;
                createUserDto.Auth0UserId = createLoginResponse.User.UserId;
                var createSubscriberResult = await _subscriberService.CreateSubscriberAsync(createUserDto);
                // if the subscriber is not created successfully, remove the associated login that was created and return a failure message
                if (!createSubscriberResult)
                {
                    _syslog.LogError($"IdentityController:CreateUserAsync An error occurred creating the user with email {createUserDto.Email} ");
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
                    return Ok(new BasicResponseDto() { StatusCode = 200, Description = createLoginResponse.Message, Data = createLoginResponse.User });
                }
            }
        }
    }
}