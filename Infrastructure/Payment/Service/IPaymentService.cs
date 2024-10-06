using Application.DTOs.Payment;
using Application.DTOs.PaymentHistory;
using Infrastructure.Vnpay.Response;

namespace Infrastructure.Payment.Service
{
    public interface IPaymentService
    {
        PaymentLink GetPaymentLink(CreatePayment request);
        Task<string> ProcessVnpayPaymentReturn(VnpayResponse response);
        Task<PaymentReturnDTO> GetPaymentDetail(string apiId);
    }
}