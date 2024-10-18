using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.NotificationDetails
{
    public class NotificationDetailRequest
    {
        [Required]
        public Guid AccountId { get; set; }
        [Required]
        public Guid NotificationId { get; set; }
    }
}
