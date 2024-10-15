using Application.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.PaymentHistory
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentHistoryController : ControllerBase
    {
        private readonly IPaymentHistoryService _service;

        public PaymentHistoryController(IPaymentHistoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get Payment History by Admin
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="CustomerName"></param>
        /// <param name="PhoneNumber"></param>
        /// <param name="Email"></param>
        /// <param name="BarId"></param>
        /// <param name="PaymentDate"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] [Required] int Status, [FromQuery] string? CustomerName, [FromQuery] string? PhoneNumber, [FromQuery] string? Email, [FromQuery] Guid? BarId, [FromQuery] DateTime? PaymentDate, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10) {
            var response = await _service.Get(Status, CustomerName, PhoneNumber, Email, BarId, PaymentDate, PageIndex, PageSize);
            return Ok(new { totalPage = response.totalPage, response = response.response });
        }

        /// <summary>
        /// Get Payment History By CustomerId
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <param name="Status"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet("{CustomerId}")]
        public async Task<IActionResult> Get(Guid CustomerId, [FromQuery] int? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var response = await _service.GetByCustomerId(CustomerId, Status, PageIndex, PageSize);
            return Ok(new { totalPage = response.totalPage, response = response.response });
        }
    }
}
