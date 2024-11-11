using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class EventVoucher
    {
        [Key]
        public Guid EventVoucherId { get; set; }
        public Guid EventId { get; set; }
        public string EventVoucherName { get; set; }
        public string VoucherCode { get; set; }
        public double Discount { get; set; }
        public double MaxPrice { get; set; }
        public int? Quantity { get; set; }
        public bool Status { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }
}
