using Application.DTOs.Account;
using Application.DTOs.Booking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public class PaymentDetailResponse
    {
        public Guid PaymentHistoryId { get; set; } = Guid.NewGuid();
        public Guid AccountId { get; set; }
        public Guid BookingId { get; set; }
        public string ProviderName { get; set; }
        public string TransactionCode { get; set; }
        public DateTime PaymentDate { get; set; }
        public double PaymentFee { get; set; }
        public double TotalPrice { get; set; }
        public string? Note { get; set; }
        public int Status { get; set; }
        public virtual CustomerAccountResponse? Account { get; set; }
        public virtual BookingResponse? Booking { get; set; }
    }
}
