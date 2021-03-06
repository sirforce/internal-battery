﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyLib.Domain.Models.CrossChq;

namespace UpDiddyApi.ApplicationCore.Services.CrossChq
{
    public class CrossChqWebClient : ICrossChqWebClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public CrossChqWebClient(IHttpClientFactory httpClientFactory, ILogger<CrossChqWebClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(CrossChqWebClient));
            _logger = logger;
        }

        public async Task<string> PostReferenceRequestAsync(ReferenceRequestDto request)
        {
            try
            {
                _logger.LogInformation("About to post a new Reference Request for {email}", request?.Candidate?.Email);

                var responseMessage = await _httpClient
                    .PostAsJsonAsync("partners/reference-request/", request);

                var content = await GetContent(responseMessage);

                var json = JObject.Parse(content);
                return json["request_id"]?.ToString();
            }
            catch (AlreadyExistsException aee)
            {
                _logger.LogError(aee, "Error occurred while retrieving a Reference Request");
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred while posting a new Reference Request");
                throw;
            }
        }

        public async Task<ReferenceResponseDto> GetReferenceRequestAsync(string referenceId)
        {
            try
            {
                _logger.LogInformation("About to retriece a Reference Request for {referenceId}", referenceId);

                var responseMessage = await _httpClient
                    .GetAsync($"partners/reference-request/{referenceId}/"); // Holy crap, don't forget that trailing slash!

                var content = await GetContent(responseMessage);

                return JsonConvert.DeserializeObject<ReferenceResponseDto>(content);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving a Reference Request");
                throw;
            }
        }

        private static async Task<string> GetContent(HttpResponseMessage responseMessage)
        {
            var statusCode = (int)responseMessage.StatusCode;
            var content = await responseMessage.Content.ReadAsStringAsync();
            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorJson = JObject.Parse(content);
                var errorMsg = errorJson["errors"]?.ToString()
                    ?? errorJson["detail"]?.ToString();

                if (statusCode == (int)System.Net.HttpStatusCode.Conflict)
                {
                    throw new AlreadyExistsException(errorMsg);
                }

                throw new HttpRequestException(
                    string.IsNullOrEmpty(errorMsg)
                    ? $"Unspecified error occured on the distant end. Status Code: {statusCode}"
                    : $"Error occured on the distant end (status code: {statusCode}): {errorMsg}");
            }

            return content;
        }
    }
}
