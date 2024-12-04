using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Fcm
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public FcmNotificationType Type { get; set; }
        public string? ImageUrl { get; set; }
        public string? MobileDeepLink { get; set; }
        public string? WebDeepLink { get; set; }
        public string? BarName { get; set; }
        public bool IsPublic { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
