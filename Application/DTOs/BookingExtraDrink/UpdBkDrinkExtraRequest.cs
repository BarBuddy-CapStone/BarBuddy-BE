using Application.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingExtraDrink
{
    public class UpdBkDrinkExtraRequest : DrinkRequest
    {
        [Range(1,2, ErrorMessage = "Không hợp lệ !")]
        public int Status { get; set; }
    }
}
