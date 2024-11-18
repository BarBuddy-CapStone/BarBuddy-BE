using Application.DTOs.Fcm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFcmService
    {
        // Device registration
        Task SaveGuestDeviceToken(string deviceToken, string platform);
        Task UpdateDeviceTokenForUser(Guid accountId, UpdateDeviceTokenRequest request);

        // Send notifications
        Task<Guid> CreateAndSendNotification(CreateNotificationRequest request);
        Task SendNotificationToUser(Guid accountId, string title, string message, Dictionary<string, string> data = null);
        Task SendBroadcastNotification(string title, string message, Dictionary<string, string> data = null);

        // Get notifications
        Task<List<NotificationResponse>> GetNotificationsForUser(Guid accountId, int page = 1, int pageSize = 20);
        Task<List<NotificationResponse>> GetPublicNotifications(string deviceToken, int page = 1, int pageSize = 20);
        Task MarkAsReadByDeviceToken(Guid notificationId, string deviceToken);

        // Mark as read
        Task MarkAsRead(Guid notificationId, Guid accountId);
        Task MarkAllAsRead(Guid accountId);
        Task MarkAllAsReadByDeviceToken(string deviceToken);
    }
}
