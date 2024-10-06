using Application.DTOs.Booking;
using Application.DTOs.PaymentHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public class PaymentReturnDTO
    {
        /// <summary>
        /// 00: Success
        /// 99: Unknown
        /// 10: Error
        /// </summary>
        public string? PaymentStatus { get; set; }
        public PaymentHistoryResponse? PaymentHistoryResponse { get; set; }
        public BookingByIdResponse? BookingByIdResponse { get; set; }
    }
}
