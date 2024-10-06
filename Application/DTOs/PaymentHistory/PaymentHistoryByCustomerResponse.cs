using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.PaymentHistory
{
    public class PaymentHistoryByCustomerResponse
    {
        public Guid BookingId { get; set; }
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? BarName { get; set; }
        public string? TransactionCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public string ProviderName { get; set; }
        public double PaymentFee { get; set; }
    }
}
