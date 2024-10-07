using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingTableRequest
    {
        public Guid BarId { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public string? Note { get; set; }
        public List<Guid>? TableIds { get; set; }
    }
}
