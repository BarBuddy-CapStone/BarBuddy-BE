using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Notification
{
    public class UpdateNotiRequest
    {
        public Guid AccountId { get; set; }
        public List<Guid> NotificationId { get; set; }
    }
}
