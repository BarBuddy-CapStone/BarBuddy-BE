using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class UpdateStsBookingExtraDrink
    {
        [Required]
        public Guid BookingExtraDrinkId { get; set; }
        [Required]
        public Guid BookingId { get; set; }
        [Required]
        public Guid DrinkId { get; set; }
        //public bool Status { get; set; }
    }
}
