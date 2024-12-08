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
        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public string? Note {  get; set; }
        public double? TotalPrice { get; set; }
        public double? AdditionalFee { get; set; }
        public Guid? CheckInStaffId { get; set; }
        public Guid? CheckOutStaffId { get; set; }
        public int NumOfTable { get; set; }
        public int NumOfPeople { get; set; }
        public string? QRTicket { get; set; }
        public DateTime ExpireAt { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public int Status { get; set; }

        public virtual ICollection<BookingDrink> BookingDrinks { get; set; }
        public virtual ICollection<BookingExtraDrink> BookingExtraDrinks { get; set; }
        public virtual ICollection<BookingTable> BookingTables { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }

        public Booking ()
        {
            BookingDate = CoreHelper.SystemTimeNow.Date;
        }
    }
}
