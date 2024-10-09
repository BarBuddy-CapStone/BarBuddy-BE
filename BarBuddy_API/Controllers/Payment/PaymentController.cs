using Application.DTOs.Payment;
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
            try
            {
                var result = await _paymentService.ProcessVnpayPaymentReturn(vnpayReturn);
                string? redirectUrl = _configuration["Vnpay:RedirectUrl"] + result;
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return CustomResult($"Internal error: {ex.Message}", 500);
            }
        }

        [HttpGet("payment-detail/{paymentHistoryId}")]
        public async Task<IActionResult> GetPaymentDetail(Guid paymentHistoryId)
        {
            var result = await _paymentService.GetPaymentDetail(paymentHistoryId);
            return CustomResult(result);
        }
    }
}
