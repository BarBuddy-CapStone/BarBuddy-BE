using Application.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool Status, [FromQuery] string? CustomerName, [FromQuery] string? PhoneNumber, [FromQuery] string? Email, [FromQuery] Guid? BarId, [FromQuery] DateTime? PaymentDate, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10) {
            var response = await _service.Get(Status, CustomerName, PhoneNumber, Email, BarId, PaymentDate, PageIndex, PageSize);
            return Ok(new { totalPage = response.totalPage, response = response.response });
        }
    }
}
