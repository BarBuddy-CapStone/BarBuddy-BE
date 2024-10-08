using Application.DTOs.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingTable
{
    public class FilterBkTableResponse
    {
        public DateTimeOffset ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public List<FilterTableResponse> Tables { get; set; }
    }
}
