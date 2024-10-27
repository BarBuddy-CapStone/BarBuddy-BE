using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BarTime
{
    public class BarTimeRequest
    {
        [Required]
        [Range(0,6, ErrorMessage ="Giá trị ngày chỉ từ 0 đến 6, vui lòng thử lại")]
        public int DayOfWeek { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
