using Application.DTOs.Booking;
using Application.IService;
using Azure;
using CoreApiResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.Booking
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : BaseController
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
            return CustomResult(new { TotalPage = responses.TotalPage, response = responses.responses} );
        }

        [HttpGet("detail/{BookingId}")]
        public async Task<IActionResult> GetBookingById(Guid BookingId) {
            var response = await _bookingService.GetBookingById(BookingId);
            return CustomResult(response);
        }

        [HttpGet("top-booking")]
        public async Task<IActionResult> GetTopBookingByCustomer([FromQuery] [Required] Guid CustomerId, [FromQuery] int NumOfBookings = 4)
        {
            var responses = await _bookingService.GetTopBookingByCustomer(CustomerId, NumOfBookings);
            return CustomResult(responses);
        }

        [HttpPatch("cancel/{BookingId}")]
        public async Task<IActionResult> CancelBooking(Guid BookingId)
        {
            var response = await _bookingService.CancelBooking(BookingId);
            if (!response)
            {
                return StatusCode(202, "Bạn chỉ có thể hủy bàn trước 2 giờ đồng hồ đến giờ phục vụ.");
            }
            return CustomResult("Hủy đặt bàn thành công");
        }

        [HttpPost("booking-table")]
        public IActionResult CreateBookingTableOnly([FromBody] BookingTableRequest request)
        {
            var response = _bookingService.CreateBookingTableOnly(request, HttpContext);
            return CustomResult("Đặt bàn thành công", response);
        }

        [HttpPost("booking-drink")]
        public IActionResult CreateBookingTableWithDrinks([FromBody] BookingDrinkRequest request)
        {
            var response = _bookingService.CreateBookingTableWithDrinks(request, HttpContext);
            return CustomResult("Đặt bàn kèm đồ uống thành công", response);
        }
    }
}
