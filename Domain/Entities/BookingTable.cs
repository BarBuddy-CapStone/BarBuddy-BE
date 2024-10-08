using Domain.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("BookingTable")]
    public class BookingTable
    {
        [Key]
        public Guid BookingTableId { get; set; } = Guid.NewGuid();
        public Guid BookingId { get; set; }
        public Guid TableId { get; set;}
        public DateTimeOffset ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
        [ForeignKey("TableId")]
        public virtual Table Table { get; set; }
    }
}
