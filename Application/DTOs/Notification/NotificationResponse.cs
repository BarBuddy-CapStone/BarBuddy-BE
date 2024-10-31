using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Notification
{
    public class NotificationResponse
    {
        public Guid NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
