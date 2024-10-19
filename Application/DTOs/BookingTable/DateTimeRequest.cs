using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingTable
{
    public class DateTimeRequest
    {
        [Required]
        public DateTimeOffset Date { get; set; }
        [Required]
        public TimeSpan Time { get; set; }
    }
}
