using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public DateTimeOffset Date { get; set; }
        public TimeSpan Time { get; set; }
    }
}
