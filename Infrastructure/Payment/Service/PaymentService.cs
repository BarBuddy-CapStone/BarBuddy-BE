using Application.DTOs.Payment;
using Application.DTOs.PaymentHistory;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Domain.Utils;
using Infrastructure.Vnpay.Config;
using Infrastructure.Zalopay.Config;
using Infrastructure.Zalopay.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Repository;
using static Domain.CustomException.CustomException;

namespace Infrastructure.Payment.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly ZalopayConfig zaloPayConfig;
        private readonly VnpayConfig vnPayConfig;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public PaymentService(
            IOptions<ZalopayConfig> zaloPayConfigOptions,
            IOptions<VnpayConfig> vnPayConfigOptions,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            zaloPayConfig = zaloPayConfigOptions.Value;
            vnPayConfig = vnPayConfigOptions.Value;
            this.httpContextAccessor = httpContextAccessor;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public PaymentLink GetPaymentLink(Guid bookingId, Guid accountId, string PaymentDestination, double totalPrice)
        {


            switch (PaymentDestination)
            {
                case "VNPAY":
                    return GetVnpayPaymentLink(bookingId, accountId, PaymentDestination, totalPrice);
                //case "ZALOPAY":
                //    return GetZalopayPaymentLink(request);
                default:
                    throw new InternalServerErrorException("Không tìm thấy phương thức thích hợp");
            }
        }


        private PaymentLink GetVnpayPaymentLink(Guid bookingId, Guid accountId, string PaymentDestination, double totalPrice)
        {
            try
            {
                var transactionCode = Guid.NewGuid();
                var paymentHistory = new PaymentHistory
                {
                    AccountId = accountId,
                    BookingId = bookingId,
                    ProviderName = PaymentDestination,
                    TransactionCode = transactionCode.ToString(),
                    PaymentDate = DateTime.Now,
                    PaymentFee = 0,
                    TotalPrice = totalPrice,
                    Status = (int)PaymentStatusEnum.Pending,
                };
                try
                {
                    unitOfWork.BeginTransaction();
                    unitOfWork.PaymentHistoryRepository.Insert(paymentHistory);
                    unitOfWork.CommitTransaction();
                }
                catch (Exception ex)
                {
                    unitOfWork.RollBack();
                    throw new InternalServerErrorException($"An Internal error occurred while creating customer: {ex.Message}");
                }
                finally
                {
                    unitOfWork.Dispose();
                }
                var outputIdParam = RandomHelper.GenerateRandomNumberString();
                var IpAddress = httpContextAccessor?.HttpContext?.Connection?.LocalIpAddress?.ToString();
                var vnpayPayRequest = new VnpayRequest(vnPayConfig.Version, vnPayConfig.TmnCode,
                            DateTime.Now, IpAddress ?? string.Empty,
                            (decimal)paymentHistory.TotalPrice, "VND",
                            "other", paymentHistory.PaymentHistoryId.ToString(),
                            vnPayConfig.ReturnUrl, transactionCode.ToString() ?? string.Empty);

                var paymentUrl = vnpayPayRequest.GetLink(vnPayConfig.PaymentUrl, vnPayConfig.HashSecret);
                var vnPayResult = new PaymentLink
                {
                    PaymentId = outputIdParam,
                    PaymentUrl = paymentUrl
                };
                return vnPayResult;
            }
            catch (Exception ex)
            {
                throw new InternalServerErrorException("An internal error: " + ex.Message);
            }
        }

        private PaymentLink GetZalopayPaymentLink(CreatePayment request)
        {
            var outputIdParam = RandomHelper.GenerateRandomNumberString();
            var zalopayPayRequest = new CreateZalopayRequest(zaloPayConfig.AppId, zaloPayConfig.AppUser,
                                        DateTime.Now.GetTimeStamp(), (long)request.RequiredAmount!,
                                        DateTime.Now.ToString("yymmdd") + "_" + outputIdParam!.ToString() ?? string.Empty,
                                        "zalopayapp", request.PaymentContent ?? string.Empty);
            zalopayPayRequest.MakeSignature(zaloPayConfig.Key1);
            (bool createZaloPayLinkResult, string? createZaloPayMessage) = zalopayPayRequest.GetLink(zaloPayConfig.PaymentUrl);
            if (createZaloPayLinkResult)
            {
                var zaloPayResult = new PaymentLink
                {
                    PaymentId = outputIdParam,
                    PaymentUrl = createZaloPayMessage
                };
                return zaloPayResult;
            }
            else
            {
                throw new InternalServerErrorException("Có lỗi tại GetPaymentLink");
            }
        }

        public async Task<Guid> ProcessVnpayPaymentReturn(VnpayResponse response)
        {
            var isValidSignature = response.IsValidSignature(vnPayConfig.HashSecret);
            if (isValidSignature && !response.vnp_OrderInfo.IsNullOrEmpty())
            {
                var paymentHistory = await unitOfWork.PaymentHistoryRepository.GetByIdAsync(Guid.Parse(response.vnp_OrderInfo));
                if (paymentHistory != null)
                {
                    if (response.vnp_ResponseCode != "00")
                    {
                        paymentHistory.Status = (int)PaymentStatusEnum.Failed;
                        throw new CustomException.InvalidDataException("Payment process failed");
                    }
                    try
                    {
                        paymentHistory.Status = (int)PaymentStatusEnum.Success;
                        await unitOfWork.PaymentHistoryRepository.UpdateAsync(paymentHistory);
                        await unitOfWork.SaveAsync();
                        return paymentHistory.PaymentHistoryId;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    throw new DataNotFoundException("Can't find payment at payment service");
                }
            }
            else
            {
                throw new CustomException.InvalidDataException("Invalid signature in response");
            }
        }

        public async Task<PaymentDetailResponse> GetPaymentDetail(Guid paymentHistoryId)
        {
            var paymentHistory = (await unitOfWork.PaymentHistoryRepository.GetAsync(
                filter: p => p.PaymentHistoryId == paymentHistoryId,
                includeProperties: "Account,Booking.BookingTables,Booking.BookinDrinks")).FirstOrDefault();
            if (paymentHistory == null) throw new DataNotFoundException("Not found PaymentHistory");

            var response = mapper.Map<PaymentDetailResponse>(paymentHistory);

            return response;
        }
    }
}
