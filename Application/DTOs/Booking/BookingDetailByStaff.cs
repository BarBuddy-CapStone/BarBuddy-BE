using Application.DTOs.BookingDrink;
using Application.DTOs.BookingTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingDetailByStaff
    {
        public Guid BookingId { get; set; }
        public string BarName { get; set; }
        public string BarAddress { get; set; }
        public string BookingCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string QRTicket { get; set; }
        public string? Note { get; set; }
        public double? TotalPrice { get; set; }
        public double? AdditionalFee { get; set; }
        public int Status { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public DateTime CreateAt { get; set; }
        public List<BookingDrinkDetailResponse> bookingDrinksList { get; set; } = new List<BookingDrinkDetailResponse>();
        public List<BookingTableResponseByStaff> bookingTableList { get; set; } = new List<BookingTableResponseByStaff>();
    }
}
