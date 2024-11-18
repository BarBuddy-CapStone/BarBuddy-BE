using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Fcm
{
    public class TestNotificationRequest
    {
        [Required]
        public Guid AccountId { get; set; }
    }
}
