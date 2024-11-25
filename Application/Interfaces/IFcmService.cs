﻿using Application.DTOs.Fcm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFcmService
    {
        Task SaveGuestDeviceToken(string deviceToken, string platform);
        Task UpdateDeviceTokenForUser(Guid accountId, UpdateDeviceTokenRequest request);
        Task<Guid> CreateAndSendNotification(CreateNotificationRequest request);
        Task SendNotificationToUser(Guid accountId, string title, string message, Dictionary<string, string> data = null);
        Task SendBroadcastNotification(string title, string message, Dictionary<string, string> data = null);
        Task<List<NotificationResponse>> GetNotificationsForUser(Guid accountId, int page = 1, int pageSize = 20);
        Task<List<NotificationResponse>> GetPublicNotifications(string deviceToken, int page = 1, int pageSize = 20);
    }
}
