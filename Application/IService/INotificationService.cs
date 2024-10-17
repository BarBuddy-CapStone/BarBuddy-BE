using Application.DTOs.Notification;
using Application.DTOs.NotificationDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateNotification(NotificationRequest request);
        Task<List<NotificationResponse>> UpdateIsReadNoti(UpdateNotiRequest request);
        public Task<NotificationDetailResponse> GetAllNotiOfCus(Guid accountId);
    }
}
