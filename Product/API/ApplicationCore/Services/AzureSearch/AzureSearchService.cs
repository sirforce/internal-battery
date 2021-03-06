﻿
using AutoMapper;
using GeoJSON.Net.Geometry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Spatial;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.AzureSearch;
using UpDiddyLib.Domain.AzureSearchDocuments;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Services.AzureSearch
{

    public class AzureSearchService : IAzureSearchService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private UpDiddyDbContext _db { get; set; }
        private IConfiguration _configuration { get; set; }
        private ICloudStorage _cloudStorage { get; set; }
        private ILogger _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private readonly IMapper _mapper; 
        private IHangfireService _hangfireService { get; set; } 
        private ISysEmail _sysEmail; 

        public AzureSearchService(
            IHttpClientFactory httpClientFactory,
            UpDiddyDbContext context,
            IConfiguration configuration,
            ICloudStorage cloudStorage,
            IRepositoryWrapper repository,
            ILogger<SubscriberService> logger,
            IMapper mapper,            
            IHangfireService hangfireService,            
            ISysEmail sysEmail,
            IButterCMSService butterCMSService         
            )
        {           
            _httpClientFactory = httpClientFactory;
            _db = context;
            _configuration = configuration;
            _cloudStorage = cloudStorage;
            _repository = repository;
            _logger = logger;
            _mapper = mapper;            
            _hangfireService = hangfireService;             
            _sysEmail = sysEmail;          
        }



        #region G2
 
        public async Task<AzureIndexResult> AddOrUpdateG2(G2SDOC g2)
        {
            return await SendG2Request(g2, "upload");
   
        }

        public async Task<AzureIndexResult> DeleteG2(G2SDOC g2)
        {
            return await SendG2Request(g2, "delete");
 
        }

        public async Task<AzureIndexResult> DeleteG2Bulk(List<G2SDOC> g2s)
        {
            return await SendG2RequestBulk(g2s, "delete");
        }


        public async Task<AzureIndexResult> AddOrUpdateG2Bulk(List<G2SDOC> g2s)
        {
            return await SendG2RequestBulk(g2s, "upload");        
        }

        #endregion

        #region subscriber index 
        public async Task<bool> AddOrUpdateSubscriber(Subscriber subscriber)
        {
            SendSubscriberRequest(subscriber, "upload");            
            return true;
        }

        public async Task<bool> DeleteSubscriber(Subscriber subscriber)
        {
            SendSubscriberRequest(subscriber, "delete");            
            return true;
        }

        #endregion

        #region Recruiter index 
        public async Task<bool> AddOrUpdateRecruiter(Recruiter recruiter)
        {
            SendRecruiterRequest(recruiter, "upload");
            return true;
        }

        public async Task<bool> DeleteRecruiter(Recruiter recruiter)
        {
            SendRecruiterRequest(recruiter, "delete");
            return true;
        }

        #endregion



        #region Candidate


        public async Task<AzureIndexResult> AddOrUpdateCandidate(CandidateSDOC candidate)
        {
            return await SendCandidateRequest(candidate, "upload");

        }

        public async Task<AzureIndexResult> DeleteCandidate(CandidateSDOC candidate)
        {
            return await SendCandidateRequest(candidate, "delete");

        }

        public async Task<AzureIndexResult> DeleteCandidateBulk(List<CandidateSDOC> candidates)
        {
            return await SendCandidateRequestBulk(candidates, "delete");
        }


        public async Task<AzureIndexResult> AddOrUpdateCandidateBulk(List<CandidateSDOC> candidates)
        {
            return await SendCandidateRequestBulk(candidates, "upload");
        }




        #endregion


        #region helper functions 


        private async Task<AzureIndexResult> SendCandidateRequestBulk(List<CandidateSDOC> candidateDocs, string cmd)
        {
            string index = _configuration["AzureSearch:CandidateIndexName"];
            SDOCRequest<CandidateSDOC> docs = new SDOCRequest<CandidateSDOC>(); 
            foreach (CandidateSDOC doc in candidateDocs)
            {         
                doc.SearchAction = cmd;
                docs.value.Add(doc);
            }
            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" });
            AzureIndexResult rval = await SendSearchIndexRequest(index, cmd, Json);
            return rval;
        }

        private async Task<AzureIndexResult> SendCandidateRequest(CandidateSDOC candidateDoc, string cmd)
        {
            string index = _configuration["AzureSearch:CandidateIndexName"];
            SDOCRequest<CandidateSDOC> docs = new SDOCRequest<CandidateSDOC>();
            candidateDoc.SearchAction = cmd;
            docs.value.Add(candidateDoc);

            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" });
            AzureIndexResult rval = await SendSearchIndexRequest(index, cmd, Json);
            return rval;
        }



        private async Task<AzureIndexResult> SendG2RequestBulk(List<G2SDOC> g2s, string cmd)
        {
            string index = _configuration["AzureSearch:G2IndexName"];
            SDOCRequest<G2SDOC> docs = new SDOCRequest<G2SDOC>();
            List<Guid> profileGuidList = new List<Guid>();
            foreach ( G2SDOC g2 in g2s)
            {
                profileGuidList.Add(g2.ProfileGuid);
                g2.SearchAction = cmd;
                docs.value.Add(g2);
            }                       
            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" });            
            AzureIndexResult rval = await SendSearchIndexRequest(index, cmd, Json);
            return rval;
        }

        private async Task<AzureIndexResult> SendG2Request(G2SDOC g2, string cmd)
        {            
            string index = _configuration["AzureSearch:G2IndexName"];
            SDOCRequest<G2SDOC> docs = new SDOCRequest<G2SDOC>();                            
            g2.SearchAction = cmd;
            docs.value.Add(g2);
 
            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs, new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" }); 
            AzureIndexResult rval = await SendSearchIndexRequest(index, cmd, Json);
            return rval;           
        }

        private async Task<bool> SendRecruiterRequest(Recruiter recruiter, string cmd)
        {
            // fire and forget 
            Task.Run(() => {
                string index = _configuration["AzureSearch:RecruiterIndexName"];
                SDOCRequest<RecruiterSDOC> docs = new SDOCRequest<RecruiterSDOC>();
                RecruiterSDOC doc = _mapper.Map<RecruiterSDOC>(recruiter);
                doc.SearchAction = cmd;
                docs.value.Add(doc);
                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs);         
                SendSearchIndexRequest(index, cmd, Json);
            });
            return true;
        }

        private async Task<bool> SendSubscriberRequest(Subscriber subscriber, string cmd)
        {
            // fire and forget 
            Task.Run(() => {
                string index = _configuration["AzureSearch:G2IndexName"];
                SDOCRequest<SubscriberSDOC> docs = new SDOCRequest<SubscriberSDOC>();
                SubscriberSDOC doc = _mapper.Map<SubscriberSDOC>(subscriber);
                doc.SearchAction = cmd;
                docs.value.Add(doc);
                string Json = Newtonsoft.Json.JsonConvert.SerializeObject(docs);
                SendSearchIndexRequest(index, cmd, Json);         
            });
            return true;
        }
        
        private async Task<AzureIndexResult> SendSearchIndexRequest(string indexName, string cmd, string jsonDocs )
        {
            string ResponseJson = string.Empty;
            AzureIndexResult rVal = new AzureIndexResult();
            try
            {
                string SearchBaseUrl = _configuration["AzureSearch:SearchServiceBaseUrl"];
                string SearchIndexVersion = _configuration["AzureSearch:SearchServiceAdminVersion"];
                string SearchApiKey = _configuration["AzureSearch:SearchServiceAdminApiKey"];
                string Url = $"{SearchBaseUrl}/indexes/{indexName}/docs/index?api-version={SearchIndexVersion}";
                _logger.LogInformation($"AzureSearchService:SendSearchRequest: url = {Url}");

                HttpClient client = _httpClientFactory.CreateClient(Constants.HttpPostClientName);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Url)
                {
                    Content = new StringContent(jsonDocs)
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Headers.Add("api-key", SearchApiKey);

                HttpResponseMessage SearchResponse = AsyncHelper.RunSync<HttpResponseMessage>(() => client.SendAsync(request));
                ResponseJson = AsyncHelper.RunSync<string>(() => SearchResponse.Content.ReadAsStringAsync());

                _logger.LogInformation($"AzureSearchService:SendSearchRequest: json = {jsonDocs}");
                _logger.LogInformation($"AzureSearchService:SendSearchRequest: response = {ResponseJson}");

                // capture statuscode for return value 
                rVal.StatusCode = SearchResponse.StatusCode;
                // return boolean for overall status and return status msg in the strong box 
                if (SearchResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {         
                    
                    rVal.DOCResults = Newtonsoft.Json.JsonConvert.DeserializeObject<AzureIndexDOCResults>(ResponseJson);
                    if (cmd != "delete")
                       rVal.StatusMsg = $"Indexed On {Utils.ISO8601DateString(DateTime.UtcNow)}";
                    else
                        rVal.StatusMsg = $"Deleted On {Utils.ISO8601DateString(DateTime.UtcNow)}"; 
                    return rVal;
                }                    
                else
                {
                    rVal.StatusMsg = $"StatusCode = {SearchResponse.StatusCode} ResponseJson = {ResponseJson} "; 
                    return rVal;
                }                    
            }
            catch ( Exception ex )
            {
                string StatusMsg = $"AzureSearchService:SendSearchRequest: Error: {ex.Message}  response = {ResponseJson}";
                _logger.LogError(StatusMsg);
                rVal.StatusMsg = StatusMsg;
                return rVal;
            }                     
        }
        
        #endregion


   
    }
}
