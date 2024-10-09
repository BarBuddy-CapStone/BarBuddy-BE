using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingTable
{
    public class TableHoldInfo
    {
        public Guid? AccountId { get; set; }
        public bool IsHeld { get; set; }
        public DateTimeOffset HoldExpiry { get; set; }
    }
}
