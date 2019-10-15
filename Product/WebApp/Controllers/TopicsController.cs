﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using UpDiddy.Api;
using UpDiddyLib.Dto;
using UpDiddy.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using UpDiddy.Services.ButterCMS;
using UpDiddy.ViewModels.ButterCMS;
using ButterCMS.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UpDiddy.Controllers
{

    [Route("[controller]")]
    public class TopicsController : BaseController
    {
        private readonly IButterCMSService _butterService;

        public TopicsController(IApi api, IConfiguration configuration, IButterCMSService butterService)
             : base(api,configuration)
        {
            _butterService = butterService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            Dictionary<string, string> QueryParams = new Dictionary<string, string>();
            foreach (string s in HttpContext.Request.Query.Keys)
            {
                QueryParams.Add(s, HttpContext.Request.Query[s].ToString());
            }
            QueryParams.Add("levels", _configuration["ButterCMS:CareerCircleTopicsPage:Levels"]);
            PageResponse<TopicsLandingPageViewModel> TopicsPage = await _butterService.RetrievePageAsync<TopicsLandingPageViewModel>("/topics", QueryParams);

            List<TrainingTopicViewModel> trainingTopics = new List<TrainingTopicViewModel>();
            foreach ( TrainingVendorViewModel tv in TopicsPage.Data.Fields.TrainingVendors )
            {
                foreach (TrainingTopicViewModel ttv in tv.TrainingTopicsList)
                    trainingTopics.Add(ttv);
            }
            // Sort the list 
            trainingTopics = trainingTopics.OrderBy(t => t.SortOrder).ToList();


            if (TopicsPage == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            // TODO: We don't have the Partners linked to the courses that they offer... 
            TopicsLandingPageViewModel TopicsViewModel = new TopicsLandingPageViewModel {
                 HeroHeader = TopicsPage.Data.Fields.HeroHeader,
                 HeroImage = TopicsPage.Data.Fields.HeroImage,
                 TrainingVendors = TopicsPage.Data.Fields.TrainingVendors,
                 HeroDescription = TopicsPage.Data.Fields.HeroDescription,
                 TrainingTopics = trainingTopics
            };

            return View("Index", TopicsViewModel);
        }

        [HttpGet("{TopicSlug}")]
        public async System.Threading.Tasks.Task<IActionResult> GetAsync(string TopicSlug)
        {         
            TopicDto Topic = await _Api.TopicBySlugAsync(TopicSlug);
            TopicViewModel TopicViewModel = new TopicViewModel(_configuration, await _Api.getCoursesByTopicSlugAsync(TopicSlug), Topic);
        
            return View("Details", TopicViewModel);
        }
        
    }
}
