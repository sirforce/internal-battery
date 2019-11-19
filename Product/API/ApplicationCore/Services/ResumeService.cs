﻿using System;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;
using UpDiddyLib.Helpers;
using UpDiddyApi.ApplicationCore.Interfaces;
using UpDiddyApi.ApplicationCore.Exceptions;
using System.Net.Http;
using System.IO;
using UpDiddyApi.ApplicationCore.Factory;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
namespace UpDiddyApi.ApplicationCore.Services
{
    public class ResumeService : IResumeService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IHangfireService _hangfireService;
        private readonly ICloudStorage _cloudStorage;
        private readonly ISubscriberService _subscriberService;
        private readonly IMapper _mapper;
        private readonly ILogger _syslog;

        public ResumeService(IHangfireService hangfireService, IRepositoryWrapper repositoryWrapper, ICloudStorage cloudStorage, ISubscriberService subscriberService, IMapper mapper, ILogger<ResumeService> syslog)
        {
            _repositoryWrapper = repositoryWrapper;
            _hangfireService = hangfireService;
            _cloudStorage = cloudStorage;
            _subscriberService = subscriberService;
            _mapper = mapper;
            _syslog = syslog;
        }

        public async Task UploadResume(Guid subscriberGuid, FileDto fileDto)
        {
            var subscriber = await _subscriberService.GetBySubscriberGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber is not found");
            if (fileDto == null)
                throw new NullReferenceException("FileDto cannot be null");
            if (string.IsNullOrEmpty(fileDto.MimeType))
                throw new NullReferenceException("FileDto.MimeType cannot be null or empty");
            if (string.IsNullOrEmpty(fileDto.FileName))
                throw new NullReferenceException("FileDto.FileName cannot be null or empty");
            if (string.IsNullOrEmpty(fileDto.Base64EncodedData))
                throw new NullReferenceException("FileDto.Base64EncodedData cannot be null or empty");
            var bytes = Convert.FromBase64String(fileDto.Base64EncodedData);
            var resumeStream = new StreamContent(new MemoryStream(bytes));
            var subscriberFiles = await _repositoryWrapper.SubscriberFileRepository.GetAllSubscriberFilesBySubscriberGuid(subscriberGuid);
            string blobName = await _cloudStorage.UploadFileAsync(String.Format("{0}/{1}/", subscriberGuid, "resume"), fileDto.FileName, await resumeStream.ReadAsStreamAsync());
            SubscriberFile subscriberFileResume = new SubscriberFile
            {
                BlobName = blobName,
                ModifyGuid = subscriberGuid,
                CreateGuid = subscriberGuid,
                CreateDate = DateTime.UtcNow,
                ModifyDate = DateTime.UtcNow,
                SubscriberId = subscriber.SubscriberId,
                MimeType = fileDto.MimeType
            };

            if (subscriberFiles.Count > 0)
            {
                SubscriberFile oldFile = subscriberFiles.Last();
                await _cloudStorage.DeleteFileAsync(oldFile.BlobName);
                oldFile.IsDeleted = 1;
            }
            await _repositoryWrapper.SubscriberFileRepository.Create(subscriberFileResume);
            await _repositoryWrapper.SaveAsync();
            await _subscriberService.QueueScanResumeJobAsync(subscriberGuid);

        }

        public async Task<UpDiddyLib.Domain.Models.FileDto> DownloadResume(Guid subscriberGuid)
        {
            SubscriberFile file = await _repositoryWrapper.SubscriberFileRepository.GetMostRecentBySubscriberGuid(subscriberGuid);
            if (file == null)
                throw new NotFoundException("Resume not found");
            UpDiddyLib.Domain.Models.FileDto resume = new UpDiddyLib.Domain.Models.FileDto();
            resume.MimeType = file.MimeType;
            resume.FileName = file.SimpleName;
            resume.ByteArrayData = Utils.StreamToByteArray(await _cloudStorage.OpenReadAsync(file.BlobName));
            return resume;
        }

        public async Task<Guid> GetResumeParse(Guid subscriberGuid)
        {
            Subscriber subscriber = await _subscriberService.GetBySubscriberGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");
            ResumeParse resumeParse = await _repositoryWrapper.ResumeParseRepository.GetLatestResumeParseRequiringMergeForSubscriber(subscriber.SubscriberId);
            if (resumeParse == null)
                throw new NotFoundException("ResumeParse not found");
            return resumeParse.ResumeParseGuid;
        }

        public async Task<UpDiddyLib.Dto.ResumeParseQuestionnaireDto> GetResumeQuestions(Guid subscriberGuid, Guid resumeParseGuid)
        {
            if (subscriberGuid == Guid.Empty || subscriberGuid == null || resumeParseGuid == Guid.Empty || resumeParseGuid == null)
                throw new NullReferenceException("SubscriberGuid and ResumeParseGuid cannot be null or empty");
            ResumeParse resumeParse = await _repositoryWrapper.ResumeParseRepository.GetResumeParseByGuid(resumeParseGuid);
            if (resumeParse == null)
                throw new NotFoundException("ResumeParse not found");
            Subscriber subscriber = await _subscriberService.GetBySubscriberGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Subscriber not found");
            UpDiddyLib.Dto.ResumeParseQuestionnaireDto resumeParseQuestionaireDto = await ResumeParseFactory.GetResumeParseQuestionnaire(_repositoryWrapper, _mapper, resumeParse);
            return resumeParseQuestionaireDto;
        }

        public async Task ResolveProfileMerge(List<string> mergeInfo, Guid subscriberGuid, Guid resumeParseGuid)
        {
            if (subscriberGuid == Guid.Empty || subscriberGuid == null || resumeParseGuid == Guid.Empty || resumeParseGuid == null || mergeInfo == null)
                throw new NullReferenceException("SubscriberGuid, ResumeParseGuid and MergeInfo cannot be null or empty");
            ResumeParse resumeParse = await _repositoryWrapper.ResumeParseRepository.GetResumeParseByGuid(resumeParseGuid);
            if (resumeParse == null)
                throw new NotFoundException("ResumeParse not found");
            Subscriber subscriber = await _subscriberService.GetBySubscriberGuid(subscriberGuid);
            await ResumeParseFactory.ResolveProfileMerge(_repositoryWrapper, _mapper, _syslog, resumeParse, subscriber, mergeInfo);
        }
    }
}
