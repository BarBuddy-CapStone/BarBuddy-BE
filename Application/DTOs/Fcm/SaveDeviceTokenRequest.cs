using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Fcm
{
    public class SaveDeviceTokenRequest
    {
        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public string DeviceToken { get; set; }

        public string Platform { get; set; } // iOS/Android
    }
}
