using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingReveueResponse
    {
        public DateTime Date {  get; set; }
        public int TotalBookingOfDay {  get; set; }
        public double TotalPrice { get; set; }
    }
}
