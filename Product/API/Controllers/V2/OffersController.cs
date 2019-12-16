﻿﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using Microsoft.AspNetCore.Authorization;
using UpDiddyLib.Domain.Models;
namespace UpDiddyApi.Controllers.V2
{
    [Route("/V2/[controller]/")]
    public class OffersController : BaseApiController
    {
        private readonly IConfiguration _configuration;

        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {

            _offerService = offerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOffers(int limit = 5, int offset = 0)
        {
            var offers = await _offerService.GetAllOffers(limit, offset);
            return Ok(offers);
        }

        [HttpGet]
        [Route("{offer:guid}")]
        public async Task<IActionResult> GetOffer(Guid offer)
        {
            var offers = await _offerService.GetOffer(offer);
            return Ok(offers);
        }

        [HttpPost]
        [Route("{offer:guid}/claim")]
        [Authorize]
        public async Task<IActionResult> ClaimOffer(Guid offer)
        {
            await _offerService.ClaimOffer(GetSubscriberGuid(), offer);
            return StatusCode(201);
        }

        [HttpPost]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> CreateOffer([FromBody] OfferDto offerDto)
        {
            await _offerService.CreateOffer(offerDto);
            return StatusCode(201);
        }

        [HttpPut]
        [Route("{offer:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> UpdateOffer(Guid offer, [FromBody]  OfferDto offerDto)
        {
            await _offerService.UpdateOffer(offer, offerDto);
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("{offer:guid}")]
        [Authorize(Policy = "IsCareerCircleAdmin")]
        public async Task<IActionResult> DeleteOffer(Guid offer)
        {
            await _offerService.DeleteOffer(offer);
            return StatusCode(204);
        }
    }
}