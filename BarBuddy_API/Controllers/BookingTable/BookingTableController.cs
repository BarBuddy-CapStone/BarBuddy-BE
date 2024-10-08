using Application.DTOs.BookingTable;
using Application.IService;
using AutoMapper;
using CoreApiResponse;
using Domain.CustomException;
using Domain.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BarBuddy_API.Controllers.BookingTable
{
    [Route("api/bookingTable")]
    [ApiController]
    public class BookingTableController : BaseController
    {
        private readonly IBookingTableService _bookingTableService;

        public BookingTableController(IBookingTableService bookingTableService)
        {
            _bookingTableService = bookingTableService;
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterDateTime ([FromQuery] FilterTableDateTimeRequest request)
        {
            try
            {
                var data = await _bookingTableService.FilterTableTypeReponse(request);
                return CustomResult("Đã tải dữ liệu", data);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
