using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingTable
{
    public class FilterTableRequest
    {
        public DateTimeOffset Date { get; set; }
        public TimeSpan Time { get; set; }
    }
}
