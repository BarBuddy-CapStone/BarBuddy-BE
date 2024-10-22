using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events.EventVoucher
{
    public class EventVoucherResponse
    {
        public Guid EventVoucherId { get; set; }
        public Guid TimeEventId { get; set; }
        public string EventVoucherName { get; set; }
        public string VoucherCode { get; set; }
        public double Discount { get; set; }
        public double MaxPrice { get; set; }
        public int Quantity { get; set; }
        public bool Status { get; set; }
    }
}
