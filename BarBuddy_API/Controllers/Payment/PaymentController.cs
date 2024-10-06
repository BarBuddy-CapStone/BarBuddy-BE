using Application.DTOs.Payment;
using CoreApiResponse;
using Infrastructure.Payment.Service;
using Infrastructure.Vnpay.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.Payment
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("")]
        public Task<IActionResult> CreatePayment([FromBody] CreatePayment request)
        {
            var response = _paymentService.GetPaymentLink(request);
            return Task.FromResult(CustomResult("Get Payment", response));
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> GetVnpayReturn([FromQuery] VnpayResponse vnpayReturn)
        {
            try
            {
                var result = await _paymentService.ProcessVnpayPaymentReturn(vnpayReturn);
                string redirectUrl = $"http://localhost:5173/payment-detail/{result}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return CustomResult($"Internal error: {ex.Message}", 500);
            }
        }

        [HttpGet("payment-detail/{apiId}")]
        public async Task<IActionResult> GetPaymentDetail(string apiId)
        {
            var result = await _paymentService.GetPaymentDetail(apiId);
            return Ok(result);
        }
    }
}
