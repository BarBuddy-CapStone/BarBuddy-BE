using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.Voucher
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : BaseController
    {
        private readonly IEventVoucherService _eventVoucherService;

        public VoucherController(IEventVoucherService eventVoucherService)
        {
            _eventVoucherService = eventVoucherService;
        }

        [HttpGet("getOneVoucher")]
        public async Task<IActionResult> GetVoucherByCode([FromQuery][Required] DateTimeOffset bookingDate, [FromQuery][Required] TimeSpan bookingTime, [FromQuery][Required] string voucherCode)
        {
            try
            {
                var response = await _eventVoucherService.GetVoucherByCode(bookingDate, bookingTime, voucherCode);
                return CustomResult("Đã tải dữ liệu.", response);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
