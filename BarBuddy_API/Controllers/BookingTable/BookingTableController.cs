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
        public async Task<IActionResult> FilterDateTime([FromQuery] FilterTableDateTimeRequest request)
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

        [HttpGet("getHoldTable/{barId}")]
        public async Task<IActionResult> GetAllHoldTable(string barId)
        {
            try
            {
                var data = await _bookingTableService.HoldTableList(Guid.Parse(barId));
                return CustomResult("Đã tải dữ liệu", data);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("holdTable")]
        public async Task<IActionResult> HoldTable([FromBody] TablesRequest request)
        {
            try
            {
                var response = await _bookingTableService.HoldTable(request);
                return CustomResult("Đang giữ chỗ", response);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("releaseTable")]
        public async Task<IActionResult> ReleaseTable([FromBody] TablesRequest request)
        {
            try
            {
                var data = await _bookingTableService.ReleaseTable(request);
                return CustomResult("Đã xóa", data);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
