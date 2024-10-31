using Application.DTOs.Notification;
using Application.DTOs.NotificationDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface INotificationDetailService
    {
        public Task<bool> CreateNotificationDetail(NotificationDetailRequest request);
        Task<bool> CreateNotificationDetailJob(NotificationDetailRequest request);
    }
}
