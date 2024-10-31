using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingCustomResponse : TopBookingResponse
    {
        public Guid AccountId { get; set; }
    }
}
