using Application.DTOs.Payment;
using Application.DTOs.PaymentHistory;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Domain.Utils;
using Infrastructure.Vnpay.Config;
using Infrastructure.Vnpay.Request;
using Infrastructure.Vnpay.Response;
using Infrastructure.Zalopay.Config;
using Infrastructure.Zalopay.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
        private readonly IBookingService bookingService;
        private static Random random;

        public PaymentService(
            IOptions<ZalopayConfig> zaloPayConfigOptions,
            IOptions<VnpayConfig> vnPayConfigOptions,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IBookingService bookingService,
            IMapper mapper)
        {
            zaloPayConfig = zaloPayConfigOptions.Value;
            vnPayConfig = vnPayConfigOptions.Value;
            this.httpContextAccessor = httpContextAccessor;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.bookingService = bookingService;
            random ??= new Random();
        }

        public PaymentLink GetPaymentLink(CreatePayment request)
        {
            switch (request.PaymentDestinationId)
            {
                case "VNPAY":
                    return GetVnpayPaymentLink(request);
                case "ZALOPAY":
                    return GetZalopayPaymentLink(request);
                default:
                    throw new InternalServerErrorException("Không tìm thấy phương thức thích hợp");
            }
        }


        private PaymentLink GetVnpayPaymentLink(CreatePayment request)
        {
            try
            {
                //var paymentHistoryRequest = mapper.Map<PaymentHistoryRequest>(request);
                //var paymentHistory = mapper.Map<PaymentHistory>(paymentHistoryRequest);
                ////paymentHistory.Status = false;
                //unitOfWork.PaymentHistoryRepository.Insert(paymentHistory);
                //unitOfWork.Save();

                //var booking = unitOfWork.BookingRepository.Get(filter: b => b.BookingId == request.BookingId,
                //    includeProperties: "BookingDrinks,BookingTables").FirstOrDefault();
                //decimal totalPrice = 0;
                //if (booking.IsIncludeDrink)
                //{
                //    totalPrice = booking.BookingDrinks.Sum(drink => (decimal)drink.ActualPrice * drink.Quantity);
                //    //tính chiết khấu
                //}

                //request.RequiredAmount = totalPrice;
                //request.PaymentContent = paymentHistory.PaymentHistoryId.ToString();
                var transactionCode = Guid.NewGuid();
                var outputIdParam = RandomHelper.GenerateRandomNumberString();
                var IpAddress = httpContextAccessor?.HttpContext?.Connection?.LocalIpAddress?.ToString();
                var vnpayPayRequest = new VnpayRequest(vnPayConfig.Version, vnPayConfig.TmnCode, 
                            DateTime.Now, IpAddress ?? string.Empty,
                            request.RequiredAmount ?? 0, request.PaymentCurrency ?? string.Empty,
                            "other", request.PaymentContent ?? string.Empty, 
                            vnPayConfig.ReturnUrl, transactionCode.ToString() ?? string.Empty);
                
                var paymentUrl = vnpayPayRequest.GetLink(vnPayConfig.PaymentUrl, vnPayConfig.HashSecret);
                var vnPayResult = new PaymentLink
                {
                    PaymentId = outputIdParam,
                    PaymentUrl = paymentUrl
                };
                return vnPayResult;
            } catch (Exception ex)
            {
                throw new InternalServerErrorException("An internal error: " +  ex.Message);
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

        public async Task<string> ProcessVnpayPaymentReturn(VnpayResponse response)
        {
            var resultData = new PaymentReturnDTO();
            var isValidSignature = response.IsValidSignature(vnPayConfig.HashSecret);
            if (isValidSignature)
            {
                if (response.vnp_ResponseCode != "00" || response.vnp_OrderInfo.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Payment process failed");
                }
                var paymentHistory = await unitOfWork.PaymentHistoryRepository.GetByIdAsync(Guid.Parse(response.vnp_OrderInfo));
                if (paymentHistory != null)
                {
                    try
                    {
                        //paymentHistory.Status = true;
                        await unitOfWork.PaymentHistoryRepository.UpdateAsync(paymentHistory);
                        await unitOfWork.SaveAsync();
                        return $"{paymentHistory.PaymentHistoryId}-{paymentHistory.BookingId}";
                    }
                    catch(Exception ex)
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

        public async Task<PaymentReturnDTO> GetPaymentDetail(string apiId)
        {
            var result = new PaymentReturnDTO();
            var ids = apiId.Split('-');
            string paymentBookingId = string.Join("-", ids[0..5]);
            string bookingId = string.Join("-", ids[5..10]);
            var booking = await bookingService.GetBookingById(Guid.Parse(bookingId));
            if (booking == null)
            {
                throw new DataNotFoundException("Can't find booking at booking service");
            }
            var paymentHistory = (await unitOfWork.PaymentHistoryRepository.GetAsync(
                filter: p => p.PaymentHistoryId == Guid.Parse(paymentBookingId))).FirstOrDefault();
            if (paymentHistory == null)
            {
                throw new DataNotFoundException("Can't find PaymentHistory");
            }

            result.PaymentHistoryResponse = mapper.Map<PaymentHistoryResponse>(paymentHistory);
            result.BookingByIdResponse = booking;
            return result;
        }
    }
}
