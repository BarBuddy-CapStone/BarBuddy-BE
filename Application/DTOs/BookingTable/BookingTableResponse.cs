using Application.DTOs.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingTable
{
    public class BookingTableResponse
    {
        public Guid TableId { get; set; }
        //public DateTimeOffset ReservationDate { get; set; }
        //public TimeSpan ReservationTime { get; set; }
        public virtual TableResponse? Table { get; set; }
    }
}
