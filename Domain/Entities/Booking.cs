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
    [Table("Booking")]
    public class Booking
    {
        [Key]
        public Guid BookingId { get; set; } = Guid.NewGuid();
        public Guid AccountId { get; set; } = Guid.NewGuid();
        public Guid BarId { get; set; } = Guid.NewGuid();
        public string BookingCode { get; set; }
        public DateTimeOffset BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public string? Note {  get; set; }
        public bool IsIncludeDrink {  get; set; }
        public int Status { get; set; }

        public virtual ICollection<BookingDrink> BookingDrinks { get; set; }
        public virtual ICollection<BookingTable> BookingTables { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }

        public Booking ()
        {
            BookingDate = CoreHelper.SystemTimeNow;
        }
    }
}
