using Application.DTOs.Payment.Momo;
using Application.DTOs.Payment.Vnpay;
using Application.Interfaces;
using CoreApiResponse;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.Payment
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        //[HttpPost("")]
        //public Task<IActionResult> CreatePayment([FromBody] CreatePayment request)
        //{
        //    var response = _paymentService.GetPaymentLink(request);
        //    return Task.FromResult(CustomResult("Get Payment", response));
        //}

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> GetVnpayReturn([FromQuery] VnpayResponse vnpayReturn)
        {
            string? redirectUrl = _configuration["Vnpay:RedirectUrl"];
            var result = Guid.Empty;
            try
            {
                result = await _paymentService.ProcessVnpayPaymentReturn(vnpayReturn);
            }
            catch (Exception ex)
            {
               throw new Exception(ex.Message, ex);
            }
            return Redirect($"{redirectUrl}{result}");
        }

        [HttpGet("momo-return")]
        public async Task<IActionResult> GetMomoReturn([FromQuery] MomoOneTimePaymentResultRequest request)
        {
            string? redirectUrl = _configuration["Vnpay:RedirectUrl"];
            var result = Guid.Empty;
            try
            {
                result = await _paymentService.ProcessMomoPaymentReturn(request);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return Redirect($"{redirectUrl}{result}");
        }

        [HttpGet("payment-detail/{paymentHistoryId}")]
        public async Task<IActionResult> GetPaymentDetail(Guid paymentHistoryId)
        {
            var result = await _paymentService.GetPaymentDetail(paymentHistoryId);
            return CustomResult(result);
        }
    }
}
