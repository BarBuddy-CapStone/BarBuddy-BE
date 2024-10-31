using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class StaffBookingReponse
    {
        public Guid BookingId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BookingCode { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public double? TotalPrice { get; set; }
        public double? AdditionalFee { get; set; }
        public int Status { get; set; }
        public string? QRTicket { get; set; }
    }
}
