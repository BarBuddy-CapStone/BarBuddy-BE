using Application.DTOs.Events.EventVoucher;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events.EventTime
{
    public class EventTimeRequest
    {
        public DateTimeOffset? Date { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
        [Range(0, 6, ErrorMessage = "Giá trị chỉ có thể từ 0 đến 6 !")]
        public int? DayOfWeek { get; set; }
        [Required]
        public List<EventVoucherRequest> EventVoucherRequest { get; set; }
    }
}
