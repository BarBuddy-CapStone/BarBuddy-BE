using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingDrink
{
    public class BookingDrinkDetailResponse
    {
        public Guid DrinkId { get; set; }
        public string DrinkName { get; set; }
        public double ActualPrice { get; set; }
        public int Quantity { get; set; }
        public string Image { get; set; }
    }
}
