using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("PaymentHistory")]
    public class PaymentHistory
    {
        [Key]
        public string PaymentHistoryId { get; set; }
        public string AccountId { get; set; }
        public string BookingId { get; set; }
        public string TransactionCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public double TotalPrice { get; set; }
        public string? Note {  get; set; }
        public bool Status { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

    }
}
