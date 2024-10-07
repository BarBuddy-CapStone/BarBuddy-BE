using Application.DTOs.Booking;
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

        [HttpPost("booking-table")]
        public IActionResult CreateBookingTableOnly([FromBody] BookingTableRequest request)
        {
            var response = _bookingService.CreateBookingTableOnly(request, HttpContext);
            return Ok(response);
        }
    }
}
