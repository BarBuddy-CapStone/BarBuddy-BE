using Application.DTOs.Booking;
using Application.DTOs.BookingExtraDrink;
using Application.IService;
using Azure;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Authorization;
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
        /// <summary>
        /// Get All Booking By CustomerId
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <param name="Status"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("{CustomerId}")]
        public async Task<IActionResult> GetAllBookingByCustomerId(Guid CustomerId, [FromQuery] int? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            try
            {
                var responses = await _bookingService.GetAllCustomerBooking(CustomerId, Status, PageIndex, PageSize);
                return CustomResult(new { TotalPage = responses.TotalPage, response = responses.responses });
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Booking By Id
        /// </summary>
        /// <param name="BookingId"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("detail/{BookingId}")]
        public async Task<IActionResult> GetBookingById(Guid BookingId)
        {
            try
            {
                var response = await _bookingService.GetBookingById(BookingId);
                return CustomResult(response);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Top Booking By Customer
        /// </summary>
        /// <param name="CustomerId"></param>
        /// <param name="NumOfBookings"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("top-booking")]
        public async Task<IActionResult> GetTopBookingByCustomer([FromQuery][Required] Guid CustomerId, [FromQuery] int NumOfBookings = 4)
        {
            try
            {
                var responses = await _bookingService.GetTopBookingByCustomer(CustomerId, NumOfBookings);
                return CustomResult(responses);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get List Booking By Staff
        /// </summary>
        /// <param name="BarId"></param>
        /// <param name="CustomerName"></param>
        /// <param name="Phone"></param>
        /// <param name="Email"></param>
        /// <param name="bookingDate"></param>
        /// <param name="bookingTime"></param>
        /// <param name="Status"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [Authorize(Roles = "STAFF")]
        [HttpGet("staff")]
        public async Task<IActionResult> GetListBookingByStaff([FromQuery] string? qrTicket, [FromQuery][Required] Guid BarId, [FromQuery] string? CustomerName, [FromQuery] string? Phone, [FromQuery] string? Email,
            [FromQuery] DateTimeOffset? bookingDate, [FromQuery] TimeSpan? bookingTime, [FromQuery] int? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            try
            {
                var responses = await _bookingService.GetListBookingAuthorized(qrTicket, BarId, CustomerName, Phone, Email, bookingDate, bookingTime, Status, PageIndex, PageSize);
                return CustomResult("Tải dữ liệu thành công", new { totalPage = responses.TotalPage, response = responses.responses });
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Booking Detail By Manager
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        [Authorize(Roles = "STAFF")]
        [HttpGet("staff/{bookingId}")]
        public async Task<IActionResult> GetBookingDetailByStaff(Guid bookingId)
        {
            var responses = await _bookingService.GetBookingDetailAuthorized(bookingId);
            return CustomResult("Tải dữ liệu thành công", responses);
        }

        [Authorize(Roles = "MANAGER")]
        [HttpGet("manager")]
        public async Task<IActionResult> GetListBookingByManager([FromQuery] string? qrTicket, [FromQuery][Required] Guid BarId, [FromQuery] string? CustomerName, [FromQuery] string? Phone, [FromQuery] string? Email,
            [FromQuery] DateTimeOffset? bookingDate, [FromQuery] TimeSpan? bookingTime, [FromQuery] int? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            try
            {
                var responses = await _bookingService.GetListBookingAuthorized(qrTicket, BarId, CustomerName, Phone, Email, bookingDate, bookingTime, Status, PageIndex, PageSize);
                return CustomResult("Tải dữ liệu thành công", new { totalPage = responses.TotalPage, response = responses.responses });
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Booking Detail By Manager
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>ssss
        [Authorize(Roles = "MANAGER")]
        [HttpGet("manager/{bookingId}")]
        public async Task<IActionResult> GetBookingDetailByManager(Guid bookingId)
        {
            try
            {
                var responses = await _bookingService.GetBookingDetailAuthorized(bookingId);
                return CustomResult("Tải dữ liệu thành công", responses);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetListBookingByAdmin([FromQuery] string? qrTicket, [FromQuery] Guid BarId, [FromQuery] string? CustomerName, [FromQuery] string? Phone, [FromQuery] string? Email,
            [FromQuery] DateTimeOffset? bookingDate, [FromQuery] TimeSpan? bookingTime, [FromQuery] int? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            try
            {
                var responses = await _bookingService.GetListBookingAuthorizedAdmin(qrTicket, BarId, CustomerName, Phone, Email, bookingDate, bookingTime, Status, PageIndex, PageSize);
                return CustomResult("Tải dữ liệu thành công", new { totalPage = responses.TotalPage, response = responses.responses });
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Booking Detail By ADMIN
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>ssss
        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/{bookingId}")]
        public async Task<IActionResult> GetBookingDetailByAdmin(Guid bookingId)
        {
            try
            {
                var responses = await _bookingService.GetBookingDetailAuthorizedAdmin(bookingId);
                return CustomResult("Tải dữ liệu thành công", responses);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update booking by staff based Status and BookingId and AdditionalFee (if it has)
        /// </summary>
        /// <param name="BookingId"></param>
        /// <param name="Status"></param>
        /// <param name="AdditionalFee"></param>
        /// <returns></returns>
        [Authorize(Roles = "STAFF")]
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateBookingByStaff([FromQuery][Required] Guid BookingId, [FromQuery][Required] int Status, [FromQuery] double? AdditionalFee)
        {
            try
            {
                await _bookingService.UpdateBookingStatus(BookingId, Status);
                return CustomResult("Cập nhật trạng thái thành công");
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Cancel one booking based BookingId
        /// </summary>
        /// <param name="BookingId"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER")]
        [HttpPatch("cancel/{BookingId}")]
        public async Task<IActionResult> CancelBooking(Guid BookingId)
        {
            try
            {
                var response = await _bookingService.CancelBooking(BookingId);
                if (!response)
                {
                    return StatusCode(202, "Bạn chỉ có thể hủy bàn trước 2 giờ đồng hồ đến giờ phục vụ.");
                }
                return CustomResult("Hủy đặt bàn thành công");
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Booking table only
        /// </summary>
        /// <param name="request">BookingTableRequest</param>
        /// <returns>Return Custom Result</returns>
        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("booking-table")]
        public async Task<IActionResult> CreateBookingTableOnly([FromBody] BookingTableRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _bookingService.CreateBookingTableOnly(request, HttpContext);
                return CustomResult("Đặt bàn thành công", response);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create Booking Table With Drinks
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("booking-drink")]
        public async Task<IActionResult> CreateBookingTableWithDrinks([FromBody] BookingDrinkRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _bookingService.CreateBookingTableWithDrinks(request, HttpContext);
                return CustomResult("Đặt bàn kèm đồ uống thành công", response);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create Booking Table With Drinks
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("booking-drink/mobile")]
        public async Task<IActionResult> CreateBookingTableWithDrinksForMobile([FromBody] BookingDrinkRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _bookingService.CreateBookingTableWithDrinks(request, HttpContext, true);
                return CustomResult("Đặt bàn kèm đồ uống thành công", response);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "STAFF")]
        [HttpPatch("update-serving/{bookingId}")]
        public async Task<IActionResult> UpdateStsBookingServing(Guid bookingId)
        {
            try
            {
                await _bookingService.UpdateStsBookingServing(bookingId);
                return CustomResult("Đã cập nhật đơn booking với trạng thái đang phục vụ !");
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "STAFF,CUSTOMER")]
        [HttpPost("extra-drink/{bookingId}")]
        public async Task<IActionResult> ExtraDrinkInServing(Guid bookingId, [FromBody] List<DrinkRequest> request)
        {
            try
            {
                await _bookingService.ExtraDrinkInServing(bookingId, request);
                return CustomResult("Đã thêm đồ uống thành công !");
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "STAFF")]
        [HttpPatch("upd-extra-drink/{bookingId}")]
        public async Task<IActionResult> UpdateExtraDrinkInServing(Guid bookingId, [FromBody] List<UpdBkDrinkExtraRequest> request)
        {
            try
            {
                await _bookingService.UpdExtraDrinkInServing(bookingId, request);
                return CustomResult("Đã cập nhật thành công !");
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "STAFF,CUSTOMER")]
        [HttpGet("get-extra-drink/{bookingId}")]
        public async Task<IActionResult> GetExtraDrinkInServing(Guid bookingId)
        {
            try
            {
                var response = await _bookingService.GetExtraBookingServing(bookingId);
                return CustomResult("Đã tải dữ liệu đồ uống !", response);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
