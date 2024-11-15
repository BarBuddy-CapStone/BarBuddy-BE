using Application.DTOs.Events.EventTime;
using Application.DTOs.Events.EventVoucher;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events
{
    public class UpdateEventRequest
    {
        [Required]
        public Guid BarId { get; set; }
        [Required]
        public string EventName { get; set; }
        [Required]
        public string Description { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? OldImages { get; set; }
        [Required]
        public bool IsEveryWeek { get; set; }
        [Required]
        public List<UpdateEventTimeRequest> UpdateEventTimeRequests { get; set; }
        public UpdateEventVoucherRequest? UpdateEventVoucherRequests { get; set; }
    }
}
