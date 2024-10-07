using Application.IService;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.Booking
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{CustomerId}")]
        public async Task<IActionResult> GetAllBookingByCustomerId (Guid CustomerId, [FromQuery] int? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _bookingService.GetAllCustomerBooking(CustomerId, Status, PageIndex, PageSize);
            return Ok(new { TotalPage = responses.TotalPage, response = responses.responses} );
        }

        [HttpGet("detail/{BookingId}")]
        public async Task<IActionResult> GetBookingById(Guid BookingId) {
            var response = await _bookingService.GetBookingById(BookingId);
            return Ok(response);
        }

        [HttpGet("top-booking")]
        public async Task<IActionResult> GetTopBookingByCustomer([FromQuery] [Required] Guid CustomerId, [FromQuery] int NumOfBookings = 4)
        {
            var responses = await _bookingService.GetTopBookingByCustomer(CustomerId, NumOfBookings);
            return Ok(responses);
        }

        [HttpGet("staff")]
        public async Task<IActionResult> GetListBookingByStaff([FromQuery][Required] Guid BarId, [FromQuery] string? CustomerName, [FromQuery] string? Phone, [FromQuery] string? Email, 
            [FromQuery] DateTimeOffset? bookingDate, [FromQuery] TimeSpan? bookingTime, [FromQuery] int? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _bookingService.GetListBookingByStaff(BarId, CustomerName, Phone, Email, bookingDate, bookingTime, Status, PageIndex, PageSize);
            return Ok(new { totalPage = responses.TotalPage, startTime = responses.startTime, endTime = responses.endTime , response = responses.responses});
        }

        [HttpGet("staff/{bookingId}")]
        public async Task<IActionResult> GetBookingDetailByStaff(Guid bookingId)
        {
            var responses = await _bookingService.GetBookingDetailByStaff(bookingId);
            return Ok(responses);
        }

        [HttpPatch("status")]
        public async Task<IActionResult> CancelBooking([FromQuery][Required] Guid BookingId, [FromQuery][Required] int Status)
        {
            await _bookingService.UpdateBookingStatus(BookingId, Status);
            return Ok("Cập nhật trạng thái thành công");
        }

        [HttpPatch("cancel/{BookingId}")]
        public async Task<IActionResult> CancelBooking(Guid BookingId)
        {
            var response = await _bookingService.CancelBooking(BookingId);
            if (!response)
            {
                return StatusCode(202, "Bạn chỉ có thể hủy bàn trước 2 giờ đồng hồ đến giờ phục vụ.");
            }
            return Ok("Hủy đặt bàn thành công");
        }
    }
}
