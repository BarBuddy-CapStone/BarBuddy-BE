using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Bar")]
    public class Bar
    {
        [Key]
        public Guid BarId { get; set; } = Guid.NewGuid();
        public string BarName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Images {  get; set; }
        public double Discount { get; set; }
        public double TimeSlot { get; set; }
        public bool Status { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<TableType> TableTypes { get; set; }
        public virtual ICollection<BarEvent> BarEvent { get; set; }
        public virtual ICollection<BarTime> BarTimes { get; set; }
        public virtual ICollection<DrinkCategory>? DrinkCategories { get; set; }
        public virtual ICollection<EmotionalDrinkCategory>? EmotionalDrinkCategories { get; set; }
    }
}
