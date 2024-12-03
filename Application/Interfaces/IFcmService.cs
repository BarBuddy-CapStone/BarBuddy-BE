using Application.DTOs.Fcm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFcmService
    {
        Task<Guid> CreateAndSendNotification(CreateNotificationRequest request);
        Task<List<NotificationResponse>> GetNotifications(Guid accountId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadNotificationCount(Guid accountId);
        Task MarkAsRead(Guid accountId, Guid notificationId);
        Task MarkAllAsRead(Guid accountId);

        Task SaveDeviceToken(string deviceToken, string platform);
        Task LinkDeviceTokenToAccount(Guid accountId, string deviceToken);
        Task UnlinkDeviceToken(string deviceToken);

        Task SendPushNotificationToAllDevices(CreateNotificationRequest request);
    }
}
