﻿using Auth0.AuthenticationApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Services.Identity.Communication;

namespace UpDiddyApi.ApplicationCore.Services.Identity.Interfaces
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(User user, bool requireEmailVerification, params Role[] userRoles);
        Task DeleteUserAsync(string userId);
    }
}
