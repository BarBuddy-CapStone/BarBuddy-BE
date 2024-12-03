using Application.DTOs.Fcm;
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
        Task<List<NotificationResponse>> GetNotifications(string deviceToken, Guid? accountId = null, int page = 1, int pageSize = 20);
        Task<int> GetUnreadNotificationCount(string deviceToken, Guid? accountId = null);
        Task<Guid> CreateAndSendNotificationToCustomer(CreateNotificationRequest request, Guid customerId);
        Task MarkAsRead(string deviceToken, Guid notificationId, Guid? accountId = null);
        Task MarkAllAsRead(string deviceToken, Guid? accountId = null);
    }
}
