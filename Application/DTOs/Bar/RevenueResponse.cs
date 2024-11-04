using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class RevenueResponse
    {
        public string BarId { get; set; }
        public double RevenueOfBar { get; set; }
        public int TotalBooking { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public List<Booking.BookingReveueResponse>? BookingReveueResponses { get; set; }
    }
}
