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
using UpDiddy.ViewModels.ButterCMS;
using UpDiddy.ViewModels.Components.Layout;
using UpDiddy.Helpers;
using UpDiddyLib.Helpers;

namespace UpDiddy.ViewComponents
{
    public class MetaTagsViewComponent : ViewComponent
    {
        private IConfiguration _configuration = null;
        private IButterCMSService _butterService = null;

        public MetaTagsViewComponent(
            IConfiguration configuration,
            IButterCMSService butterService)
        {
            _configuration = configuration;
            _butterService = butterService;
        }

        private ButterCMSBaseViewModel GetButterCMSBaseViewModel()
        {
            return new ButterCMSBaseViewModel
            {
                Title = _configuration["SEO:Meta:Title"],
                MetaDescription = _configuration["SEO:Meta:Description"],
                MetaKeywords = _configuration["SEO:Meta:Keywords"],
                OpenGraphTitle = _configuration["SEO:OpenGraph:Title"],
                OpenGraphDescription = _configuration["SEO:OpenGraph:Description"],
                OpenGraphImage = _configuration["SEO:OpenGraph:Image"]
            };
        }

        public async Task<IViewComponentResult> InvokeAsync(string PagePath)
        {
            
            var response = await _butterService.RetrievePageAsync<ButterCMSBaseViewModel>(PagePath);

            ButterCMSBaseViewModel ButterViewModel = GetButterCMSBaseViewModel();

            // overwrite defaults if butter provides them
            if (response != null)
            {
                ButterViewModel.Title = ButterViewModel.Title.GetNonEmptyStringOrDefault(response.Data.Fields.Title);
                ButterViewModel.MetaDescription = ButterViewModel.MetaDescription.GetNonEmptyStringOrDefault(response.Data.Fields.MetaDescription);
                ButterViewModel.MetaKeywords = ButterViewModel.MetaKeywords.GetNonEmptyStringOrDefault(response.Data.Fields.MetaKeywords);

                ButterViewModel.OpenGraphTitle = ButterViewModel.OpenGraphTitle.GetNonEmptyStringOrDefault(response.Data.Fields.OpenGraphTitle);
                ButterViewModel.OpenGraphDescription = ButterViewModel.OpenGraphDescription.GetNonEmptyStringOrDefault(response.Data.Fields.OpenGraphDescription);
                ButterViewModel.OpenGraphImage = ButterViewModel.OpenGraphImage.GetNonEmptyStringOrDefault(response.Data.Fields.OpenGraphImage);
                
            }

            // allow application to ovewrite meta tags
            ButterViewModel.Title = ButterViewModel.Title.GetNonEmptyStringOrDefault((string)ViewData[Constants.Seo.TITLE]);
            ButterViewModel.MetaDescription = ButterViewModel.MetaDescription.GetNonEmptyStringOrDefault((string)ViewData[Constants.Seo.META_DESCRIPTION]);
            ButterViewModel.MetaKeywords = ButterViewModel.MetaKeywords.GetNonEmptyStringOrDefault((string)ViewData[Constants.Seo.META_KEYWORDS]);

            ButterViewModel.OpenGraphTitle = ButterViewModel.OpenGraphTitle.GetNonEmptyStringOrDefault((string)ViewData[Constants.Seo.OG_TITLE]);
            ButterViewModel.OpenGraphDescription = ButterViewModel.OpenGraphDescription.GetNonEmptyStringOrDefault((string)ViewData[Constants.Seo.OG_DESCRIPTION]);
            ButterViewModel.OpenGraphImage = ButterViewModel.OpenGraphImage.GetNonEmptyStringOrDefault((string)ViewData[Constants.Seo.OG_IMAGE]);

            return View(ButterViewModel);

        }

    }
}