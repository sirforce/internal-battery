﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyApi.ApplicationCore.Exceptions;
using UpDiddyApi.ApplicationCore.Interfaces.Business;
using UpDiddyApi.ApplicationCore.Interfaces.Repository;
using UpDiddyApi.Models;
using UpDiddyLib.Domain.Models;

namespace UpDiddyApi.ApplicationCore.Services
{
    public class SubscriberNotificationService : ISubscriberNotificationService
    {
        private ILogger<SubscriberNotificationService> _logger { get; set; }
        private IRepositoryWrapper _repository { get; set; }
        private IMapper _mapper { get; set; }

        public SubscriberNotificationService(ILogger<SubscriberNotificationService> logger, IRepositoryWrapper repository, IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<bool> DeleteSubscriberNotification(Guid subscriberGuid, Guid subscriberNotificationGuid)
        {
            bool isOperationSuccessful = false;
            try
            {
                var subscriberNotification = await _repository.SubscriberNotificationRepository.GetSubscriberNotificationByIdentifiersAsync(subscriberGuid, subscriberNotificationGuid);
                if (subscriberNotification == null)
                    throw new NotFoundException($"Unrecognized subscriber notification; subscriberGuid: {subscriberGuid}, subscriberNotificationGuid: {subscriberNotificationGuid}");
                subscriberNotification.IsDeleted = 1;
                subscriberNotification.ModifyDate = DateTime.UtcNow;
                subscriberNotification.ModifyGuid = Guid.Empty;
                await _repository.SubscriberNotificationRepository.SaveAsync();
                isOperationSuccessful = true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"SubscriberNotificationService.DeleteSubscriberNotification: An error occured while attempting to delete the subscriber notification. Message: {e.Message}", e);
            }
            return isOperationSuccessful;
        }

        public async Task<NotificationListDto> GetNotifications(Guid subscriberGuid, int limit = 10, int offset = 0, string sort = "modifyDate", string order = "descending")
        {
            List<NotificationDto> notifications = await _repository.StoredProcedureRepository.GetSubscriberNotifications(subscriberGuid, limit, offset, sort, order);
            return _mapper.Map<NotificationListDto>(notifications);
        }

        public async Task<NotificationDto> GetNotification(Guid subscriberGuid, Guid notificationGuid)
        {
            // Locate the subscribers notification 
            var notification = _repository.SubscriberNotificationRepository.GetAll()
              .Include(n => n.Notification)
              .Where(sn => sn.IsDeleted == 0 && sn.SubscriberNotificationGuid == notificationGuid)
              .FirstOrDefault();

            if (notification == null)
                throw new NotFoundException("Cannot find notification");

            var subscriber = await _repository.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Cannot find subscriber");
            // check to see if owner is correct 
            if (notification.SubscriberId != subscriber.SubscriberId)
                throw new AccessViolationException("Notification is not for specified subscriber");

            NotificationDto rval = _mapper.Map<NotificationDto>(notification);
            return (rval);
        }

        public async Task<Guid> CreateSubscriberNotification(Guid subscriberGuid, Guid notificationGuid, Guid recipientGuid)
        {
            // Locate the notification 
            Notification notification = await _repository.NotificationRepository.GetByGuid(notificationGuid);
            if (notification == null)
                throw new NotFoundException("Cannot find notification");
            
            // Locate the subscriber 
            Subscriber subscriber = await _repository.SubscriberRepository.GetByGuid(recipientGuid);
            if (subscriber == null)
                throw new NotFoundException("Cannot find subscriber");

            SubscriberNotification newNotification = new SubscriberNotification()
            {
                CreateDate = DateTime.UtcNow,
                CreateGuid = subscriberGuid,
                HasRead = 0,
                IsDeleted = 0,
                NotificationId = notification.NotificationId,
                SubscriberId = subscriber.SubscriberId,
                SubscriberNotificationGuid = Guid.NewGuid()
            };

            await _repository.SubscriberNotificationRepository.Create(newNotification);
            await _repository.SubscriberNotificationRepository.SaveAsync();

            return newNotification.SubscriberNotificationGuid;
        }

        public async Task<bool> UpdateSubscriberNotification(Guid subscriberGuid, Guid subscriberNotificationGuid, NotificationDto notificationUpdate)
        {
            if (notificationUpdate == null)
                throw new FailedValidationException("Update notification is required");

            // Locate the subscriber 
            Subscriber subscriber = await _repository.SubscriberRepository.GetByGuid(subscriberGuid);
            if (subscriber == null)
                throw new NotFoundException("Cannot find subscriber");

            SubscriberNotification subscriberNotification = 
                await _repository.SubscriberNotificationRepository.GetSubscriberNotificationByIdentifiersAsync(subscriberGuid, subscriberNotificationGuid);

            if (subscriberNotification == null)
                throw new NotFoundException("Cannot find notification");

            subscriberNotification.HasRead = notificationUpdate.HasRead;
            subscriberNotification.ModifyDate = DateTime.UtcNow;
            subscriberNotification.ModifyGuid = subscriberGuid;

            _repository.SubscriberNotificationRepository.Update(subscriberNotification);
            await _repository.SubscriberNotificationRepository.SaveAsync();

            return true;
        }
    }
}