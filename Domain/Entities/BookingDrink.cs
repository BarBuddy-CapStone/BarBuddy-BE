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
        public string BookingDrinkId { get; set; }
        public string DrinkId { get; set; }
        public string BookingId { get; set; }
        public double ActualPrice { get; set; }
        public int Quantity { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [ForeignKey("DrinkId")]
        public virtual Drink Drink { get; set; }
    }
}
