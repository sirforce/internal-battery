﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddy.Api;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.Components.Layout;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddy.ViewComponents
{
    public class PublicSiteNavigationViewComponent : ViewComponent
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        // todo: part of refactor of API updiddy
        private IApi _Api = null;
        private IConfiguration _configuration = null;
        private IDistributedCache _cache = null;
        private IButterCMSService _butterService = null;
        private ISysEmail _sysEmail = null;

        public PublicSiteNavigationViewComponent(
            IApi api, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor, 
            IDistributedCache cache,
            IButterCMSService butterService,
            ISysEmail sysEmail)
        {
            _Api = api;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _butterService = butterService;
            _sysEmail = sysEmail;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            Guid SubscriberGuid = Guid.Empty;
            foreach(Claim claim in HttpContext.User.Claims)
            {
                if (claim.Type.Equals(ClaimTypes.NameIdentifier))
                    SubscriberGuid = Guid.Parse(claim.Value);
            }

            int NotificationCount = 0;
            if (User.Identity.IsAuthenticated && SubscriberGuid != Guid.Empty)
            {
                SubscriberDto Subscriber = await _Api.SubscriberAsync(SubscriberGuid, false);
                foreach(NotificationDto Notification in Subscriber.Notifications)
                {
                    if (Notification.HasRead == 0)
                        NotificationCount++;
                }
            }



            var ButterResponse = await _butterService.RetrieveContentFieldsAsync<PublicSiteNavigationViewModel<PublicSiteNavigationMenuItemViewModel>>(
                "CareerCirclePublicSiteNavigation",
                new string[1] { _configuration["ButterCMS:CareerCirclePublicSiteNavigation:Slug"] }, 
                new Dictionary<string, string> {
                    {
                        "levels", _configuration["ButterCMS:CareerCirclePublicSiteNavigation:Levels"]
                    }
                });

            if(ButterResponse != null)
            {
                PublicSiteNavigationMenuItemViewModel PublicSiteNavigation = FindDesiredNavigation(ButterResponse, "CareerCirclePublicSiteNavigation");
                PublicSiteNavigation.NotificationCount = NotificationCount;
                return View(PublicSiteNavigation);
            }
            return View("Error");
            
        }

        private PublicSiteNavigationMenuItemViewModel FindDesiredNavigation(
            PublicSiteNavigationViewModel<PublicSiteNavigationMenuItemViewModel> CCNavigation, 
            string NavigationLabel)
        {
            foreach(PublicSiteNavigationMenuItemViewModel MenuItem in CCNavigation.CareerCirclePublicSiteNavigationRoot)
            {
                if (MenuItem.Label.Equals(NavigationLabel))
                    return MenuItem;
            }
            return null;
        }

    }
}
