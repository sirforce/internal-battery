﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpDiddyLib.Dto;

namespace UpDiddyApi.ApplicationCore.Interfaces.Business
{
    public interface INotificationService
    {
        Task<Guid> CreateNotification(Guid subscriberGuid, NewNotificationDto newNotificationDto);

        Task DeleteNotification(Guid subscriberGuid, Guid notificationGuid);



        Task UpdateNotification(Guid subscriberGuid, NotificationDto notification, Guid notificationGuid);
    }
}