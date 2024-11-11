using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingDrinkRequest : BookingTableRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "PaymentDestination is required.")]
        public string? PaymentDestination { get; set; }
        public string? VoucherCode { get; set; }
        public List<DrinkRequest>? Drinks { get; set; }
    }

    public class DrinkRequest()
    {
        public Guid DrinkId { get; set; }
        public int Quantity { get; set; }
    }
}
