using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class TopBookingResponse
    {
        public Guid BookingId { get; set; }
        public string BarName { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public string BookingCode { get; set; }
        public int Status { get; set; }
        public DateTime CreateAt { get; set; }
        public string Image {  get; set; }
        public bool? IsRated { get; set; }
    }
}
