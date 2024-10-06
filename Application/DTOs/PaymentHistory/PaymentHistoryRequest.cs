using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PaymentHistory
{
    public class PaymentHistoryRequest
    {
        public Guid AccountId { get => Guid.Parse("550e8400-e29b-41d4-b777-446655440001"); }
        public Guid BookingId { get; set; }
        public string ProviderName { get; set; }
        public string TransactionCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public double PaymentFee { get => 0; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }
        public bool Status { get; set; }
    }
}
