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

        /// <summary>
        /// Get Vnpay Return
        /// </summary>
        /// <param name="vnpayReturn"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> GetVnpayReturn([FromQuery] VnpayResponse vnpayReturn)
        {
            string? redirectUrl = _configuration["Payment:SuccessUrl"];
            var result = Guid.Empty;
            try
            {
                result = await _paymentService.ProcessVnpayPaymentReturn(vnpayReturn);
            }
            catch (InvalidDataException)
            {
                redirectUrl = _configuration["Payment:FailedUrl"];
            }
            catch (Exception ex)
            {
                redirectUrl = _configuration["Payment:Error"];
            }
            return Redirect($"{redirectUrl}{result}");
        }
        /// <summary>
        /// Get Momo Return
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("momo-return")]
        public async Task<IActionResult> GetMomoReturn([FromQuery] MomoOneTimePaymentResultRequest request)
        {
            string? redirectUrl = _configuration["Payment:SuccessUrl"];
            var result = Guid.Empty;
            try
            {
                result = await _paymentService.ProcessMomoPaymentReturn(request);
            }
            catch (InvalidDataException)
            {
                redirectUrl = _configuration["Payment:FailedUrl"];
            }
            catch (Exception ex)
            {
                redirectUrl = _configuration["Payment:Error"];
            }
            return Redirect($"{redirectUrl}{result}");
        }

        /// <summary>
        /// Get Payment Detail by paymentHistoryId
        /// </summary>
        /// <param name="paymentHistoryId"></param>
        /// <returns></returns>
        [HttpGet("payment-detail/{paymentHistoryId}")]
        public async Task<IActionResult> GetPaymentDetail(Guid paymentHistoryId)
        {
            var result = await _paymentService.GetPaymentDetail(paymentHistoryId);
            return CustomResult(result);
        }
    }
}
