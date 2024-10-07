using Application.DTOs.BookingDrink;
using Application.DTOs.BookingTable;
using Application.DTOs.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingResponse
    {
        public Guid BookingId { get; set; }
        public string BarName { get; set; }
        public string BarAddress { get; set; }
        public string BookingCode { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public DateTime CreateAt { get; set; }
        public List<string> Images { get; set; }
        public int Status { get; set; }
        public List<BookingTableResponse> BookingTables { get; set; } = new List<BookingTableResponse>();
        public List<BookingDrinkDetailResponse> BookinDrinks { get; set; } = new List<BookingDrinkDetailResponse>();
    }
}
