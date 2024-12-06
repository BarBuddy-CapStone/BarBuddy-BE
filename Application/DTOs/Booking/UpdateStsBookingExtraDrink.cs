using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class UpdateStsBookingExtraDrink
    {
        public Guid BookingExtraDrinkId { get; set; }
        public Guid BookingId { get; set; }
        public Guid DrinkId { get; set; }
        //public bool Status { get; set; }
    }
}
