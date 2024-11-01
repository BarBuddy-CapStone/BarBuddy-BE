using Application.DTOs.Payment;
using Application.DTOs.Payment.Momo;
using Application.DTOs.Payment.Vnpay;

namespace Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDetailResponse> GetPaymentDetail(Guid paymentHistoryId);
        PaymentLink GetPaymentLink(Guid bookingId, Guid accountId, string PaymentDestination, double totalPrice, bool isMobile = false);
        Task<Guid> ProcessVnpayPaymentReturn(VnpayResponse response);
        Task<Guid> ProcessMomoPaymentReturn(MomoOneTimePaymentResultRequest request);
    }
}