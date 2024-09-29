using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("BookingDrink")]
    public class BookingDrink
    {
        [Key]
        public Guid BookingDrinkId { get; set; } = Guid.NewGuid();
        public Guid DrinkId { get; set; }
        public Guid BookingId { get; set; }
        public double ActualPrice { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [ForeignKey("DrinkId")]
        public virtual Drink Drink { get; set; }
    }
}
