using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Feedback")]
    public class Feedback
    {
        [Key]
        public string FeedbackId { get; set; }
        public string AccountId {  get; set; }
        public string BookingId { get; set; }
        public string BarId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset FeedbackDate { get; set; }

        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}
