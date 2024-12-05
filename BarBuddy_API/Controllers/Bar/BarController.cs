using Application.DTOs.Bar;
using Application.IService;
using CoreApiResponse;
using Domain.Common;
using Domain.CustomException;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BarBuddy_API.Controllers.Bar
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BarController : BaseController
    {
        private readonly IBarService _barService;

        public BarController(IBarService barService)
        {
            _barService = barService;
        }
        /// <summary>
        /// Get All Bar
        /// </summary>
        /// <returns></returns>
        ///

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/barmanager")]
        public async Task<IActionResult> GetAllBar([FromQuery] ObjectQueryCustom query)
        {
            try
            {
                var response = await _barService.GetAllBar(query);
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get All Bar With Feedback
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api/v1/bars")]
        public async Task<IActionResult> GetAllBarWithFeedback([FromQuery] ObjectQuery query)
        {
            try
            {
                var response = await _barService.GetAllBarWithFeedback(query);
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get All Bars Available with Date
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/v1/bars/{datePicker}")]
        public async Task<IActionResult> GetAllBarByDate(DateTime datePicker, [FromQuery] ObjectQuery query)
        {
            try
            {
                var response = await _barService.GetAllAvailableBars(datePicker, query);
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Bar With Feedback By barId
        /// </summary>
        /// <param name="barId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api/v1/bar-detail/{barId}")]
        public async Task<IActionResult> GetBarWithFeedbackById(Guid barId)
        {
            try
            {
                var response = await _barService.GetBarByIdWithFeedback(barId);
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Bar With Table By barId
        /// </summary>
        /// <param name="barId"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/bar-table/{barId}")]
        public async Task<IActionResult> GetBarWithTableById(Guid barId)
        {
            try
            {
                var response = await _barService.GetBarByIdWithTable(barId);
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Bar By Id
        /// </summary>
        /// <param name="barId"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/barProfile/{barId}")]
        public async Task<IActionResult> GetBarById(Guid barId)
        {
            try
            {
                var response = await _barService.GetBarById(barId);
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create a Bar
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPost("admin/addBar")]
        public async Task<IActionResult> CreateBar([FromBody] CreateBarRequest request)
        {
            try
            {
                await _barService.CreateBar(request);
                return CustomResult("Created Successfully");
            }
            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, HttpStatusCode.BadRequest);
            }

            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update Bar
        /// </summary>
        /// <param name="barId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("admin/updateBar")]
        public async Task<IActionResult> UpdateBar([FromBody] UpdateBarRequest request)
        {
            try
            {
                var response = await _barService.UpdateBarById(request);
                return CustomResult("Updated Successfully", response);
            }
            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, HttpStatusCode.BadRequest);
            }

            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }
        /// <summary>
        /// Get Revenue Of Bar
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("dashboard/revenueChart")]
        public async Task<IActionResult> GetRevenueOfBar([FromQuery] RevenueRequest request)
        {
            try
            {
                var response = await _barService.GetRevenueOfBar(request);
                return CustomResult("Đã tải dữ liệu", response);
            }
            catch (CustomException.UnAuthorizedException e)
            {
                return CustomResult(e.Message, HttpStatusCode.Unauthorized);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get Bar Name Id
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/dashboard/getBar")]
        public async Task<IActionResult> GetBarNameId([FromQuery] ObjectQuery query)
        {
            try
            {
                var response = await _barService.GetBarNameId(query);
                return CustomResult("Đã tải dữ liệu.", response);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult("Lỗi hệ thống !", HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get All Revenue
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("dashboard/getAllRevenue")]
        public async Task<IActionResult> GetAllRevenue()
        {
            try
            {
                var response = await _barService.GetAllRevenueBranch();
                return CustomResult("Đã tải dữ liệu", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }
        /// <summary>
        /// Get Bar Name Id Customer
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("customer/getBar")]
        public async Task<IActionResult> GetBarNameIdCustomer([FromQuery] ObjectQuery query)
        {
            try
            {
                var response = await _barService.GetBarNameId(query);
                return CustomResult("Đã tải dữ liệu.", response);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult("Lỗi hệ thống !", HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "ADMIN,MANAGER,STAFF")]
        [HttpGet("customer/getBarByAdminManager")]
        public async Task<IActionResult> GetBarNameIdAdMana([FromQuery] ObjectQuery query)
        {
            try
            {
                var response = await _barService.GetBarNameIdAdMa(query);
                return CustomResult("Đã tải dữ liệu.", response);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult("Lỗi hệ thống !", HttpStatusCode.InternalServerError);
            }
        }

        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("download-revenue")]
        public async Task<IActionResult> DownloadRevenue([FromQuery] ObjectKeyDateTimeQuery query)
        {
            try
            {
                var (fileContents, fileName) = await _barService.DownloadRevenueExcel(query);
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
