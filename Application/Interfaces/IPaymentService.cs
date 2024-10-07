using Application.DTOs.Payment;

namespace Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDetailResponse> GetPaymentDetail(Guid paymentHistoryId);
        PaymentLink GetPaymentLink(Guid bookingId, Guid accountId, string PaymentDestination, double totalPrice);
        Task<Guid> ProcessVnpayPaymentReturn(VnpayResponse response);
    }
}