using Application.DTOs.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.NotificationDetails
{
    public class NotificationDetailResponse
    {
        public Guid AccountId { get; set; }
        public string Fullname { get; set; }
        public List<NotificationResponse>? NotificationResponses { get; set; }
    }
}
