using Application.DTOs.Payment;
using Application.DTOs.Payment.Momo;
using Application.DTOs.Payment.Vnpay;
using Application.DTOs.PaymentHistory;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Azure;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Domain.Utils;
using Infrastructure.Momo.Config;
using Infrastructure.Vnpay.Config;
using Infrastructure.Zalopay.Config;
using Infrastructure.Zalopay.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Repository;
using static Domain.CustomException.CustomException;
using static Google.Apis.Requests.BatchRequest;

namespace Infrastructure.Payment.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly ZalopayConfig zaloPayConfig;
        private readonly VnpayConfig vnPayConfig;
        private readonly MomoConfig momoConfig;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public PaymentService(
            IOptions<ZalopayConfig> zaloPayConfigOptions,
            IOptions<VnpayConfig> vnPayConfigOptions,
            IOptions<MomoConfig> momoConfigOptions,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            zaloPayConfig = zaloPayConfigOptions.Value;
            vnPayConfig = vnPayConfigOptions.Value;
            momoConfig = momoConfigOptions.Value;
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
                case "ZALOPAY":
                    return GetZalopayPaymentLink(bookingId, accountId, PaymentDestination, totalPrice);
                case "MOMO":
                    return GetMomopayPaymentLink(bookingId, accountId, PaymentDestination, totalPrice);
                default:
                    throw new InternalServerErrorException("Không tìm thấy phương thức thích hợp");
            }
        }


        private PaymentLink GetVnpayPaymentLink(Guid bookingId, Guid accountId, string paymentDestination, double totalPrice)
        {
            try
            {
                var paymentHistory = CreatePaymentHistory(bookingId, accountId, paymentDestination, totalPrice);

                var outputIdParam = RandomHelper.GenerateRandomNumberString();
                var IpAddress = httpContextAccessor?.HttpContext?.Connection?.LocalIpAddress?.ToString();
                var vnpayPayRequest = new VnpayRequest(vnPayConfig.Version, vnPayConfig.TmnCode,
                            DateTime.Now, IpAddress ?? string.Empty,
                            (decimal)paymentHistory.TotalPrice, "VND",
                            "other", paymentHistory.PaymentHistoryId.ToString(),
                            vnPayConfig.ReturnUrl, paymentHistory.PaymentHistoryId.ToString() ?? string.Empty);

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

        private PaymentLink GetZalopayPaymentLink(Guid bookingId, Guid accountId, string paymentDestination, double totalPrice)
        {
            var paymentHistory = CreatePaymentHistory(bookingId, accountId, paymentDestination, totalPrice);

            var outputIdParam = RandomHelper.GenerateRandomNumberString();
            var zalopayPayRequest = new CreateZalopayRequest(zaloPayConfig.AppId, zaloPayConfig.AppUser,
                                        DateTime.Now.GetTimeStamp(), (long)paymentHistory.TotalPrice!,
                                        DateTime.Now.ToString("yymmdd") + "_" + outputIdParam!.ToString() ?? string.Empty,
                                        "zalopayapp", paymentHistory.PaymentHistoryId.ToString());
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

        private PaymentLink GetMomopayPaymentLink(Guid bookingId, Guid accountId, string paymentDestination, double totalPrice)
        {
            var paymentHistory = CreatePaymentHistory(bookingId, accountId, paymentDestination, totalPrice);

            var outputIdParam = RandomHelper.GenerateRandomNumberString();
            var momoOneTimePayRequest = new MomoOneTimePaymentRequest(momoConfig.PartnerCode, 
                    outputIdParam?.ToString() ?? string.Empty, (long)paymentHistory.TotalPrice!, 
                    outputIdParam?.ToString() ?? string.Empty, paymentHistory.PaymentHistoryId.ToString(), 
                    momoConfig.ReturnUrl, momoConfig.IpnUrl, "captureWallet", string.Empty);
            momoOneTimePayRequest.MakeSignature(momoConfig.AccessKey, momoConfig.SecretKey);
            (bool createMomoLinkResult, string? createMessage) = momoOneTimePayRequest.GetLink(momoConfig.PaymentUrl);
            if (createMomoLinkResult)
            {
                var momoPayResult = new PaymentLink
                {
                    PaymentId = outputIdParam,
                    PaymentUrl = createMessage
                };
                return momoPayResult;
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
                var paymentHistory = (await unitOfWork.PaymentHistoryRepository
                    .GetAsync(
                        filter: x => x.PaymentHistoryId == Guid.Parse(response.vnp_OrderInfo), 
                        includeProperties: "Booking"))
                    .FirstOrDefault();
                if (paymentHistory != null)
                {
                    if (response.vnp_ResponseCode != "00" && response.vnp_TransactionStatus != "00")
                    {
                        paymentHistory.Booking.Status = (int)PrefixValueEnum.Cancelled;
                        paymentHistory.Status = (int)PaymentStatusEnum.Failed;
                        await unitOfWork.PaymentHistoryRepository.UpdateAsync(paymentHistory);
                        await unitOfWork.SaveAsync();
                        throw new CustomException.InvalidDataException("Payment process failed");
                    }
                    try
                    {
                        paymentHistory.Status = (int)PaymentStatusEnum.Success;
                        paymentHistory.Booking.Status = (int)PrefixValueEnum.PendingBooking;
                        paymentHistory.TransactionCode = $"{DateTime.Now.ToString("yyyyMMdd")}-{response.vnp_TransactionNo}";
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

        public async Task<Guid> ProcessMomoPaymentReturn(MomoOneTimePaymentResultRequest request)
        {
            var isValidSignature = request.IsValidSignature(momoConfig.AccessKey, momoConfig.SecretKey);
            if (isValidSignature && !request.orderInfo.IsNullOrEmpty())
            {
                var paymentHistory = (await unitOfWork.PaymentHistoryRepository
                    .GetAsync(
                        filter: x => x.PaymentHistoryId == Guid.Parse(request.orderInfo),
                        includeProperties: "Booking"))
                    .FirstOrDefault(); 
                if (paymentHistory != null)
                {
                    if (request.resultCode != 0)
                    {
                        paymentHistory.Booking.Status = (int)PrefixValueEnum.Cancelled;
                        paymentHistory.Status = (int)PaymentStatusEnum.Failed;
                        await unitOfWork.PaymentHistoryRepository.UpdateAsync(paymentHistory);
                        await unitOfWork.SaveAsync();
                        throw new CustomException.InvalidDataException("Payment process failed");
                    }
                    try
                    {
                        paymentHistory.Status = (int)PaymentStatusEnum.Success;
                        paymentHistory.Booking.Status = (int)PrefixValueEnum.PendingBooking;
                        paymentHistory.TransactionCode = $"{DateTime.Now.ToString("yyyyMMdd")}-{request.transId}";
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

        private PaymentHistory CreatePaymentHistory(Guid bookingId, Guid accountId, string PaymentDestination, double totalPrice)
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
                return paymentHistory;
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
        }

        public async Task<PaymentDetailResponse> GetPaymentDetail(Guid paymentHistoryId)
        {
            var paymentHistory = (await unitOfWork.PaymentHistoryRepository.GetAsync(
                filter: p => p.PaymentHistoryId == paymentHistoryId,
                includeProperties: "Account,Booking.BookingTables,Booking.BookingDrinks")).FirstOrDefault();
            if (paymentHistory == null) throw new DataNotFoundException("Not found PaymentHistory");

            var response = mapper.Map<PaymentDetailResponse>(paymentHistory);

            return response;
        }
    }
}
