using Application.DTOs.BookingDrink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingByIdResponse
    {
        public Guid BookingId { get; set; }
        public Guid BarId { get; set; }
        public string BarName { get; set; }
        public string BarAddress { get; set; }
        public string BookingCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string? Note { get; set; }
        public double? TotalPrice { get; set; }
        public double? AdditionalFee { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public DateTime CreateAt { get; set; }
        public string? QrTicket { get; set; }
        public List<string> Images { get; set; }
        public int Status { get; set; }
        public bool? IsRated { get; set; }
        public List<string> TableNameList { get; set; } = new List<string>();
        public List<BookingDrinkDetailResponse> bookingDrinksList { get; set; } = new List<BookingDrinkDetailResponse>();
    }
}
