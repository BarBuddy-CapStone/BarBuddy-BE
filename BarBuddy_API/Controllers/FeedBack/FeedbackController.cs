using Application.DTOs.Request.FeedBackRequest;
using Application.IService;
using CoreApiResponse;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.FeedBack
{
    [Route("api/feedback")]
    [ApiController]

    public class FeedBack : BaseController
    {
        private readonly IFeedBackService _feedBackService;

        public FeedBack(IFeedBackService feedBackService)
        {
            _feedBackService = feedBackService;
        }

        /// <summary>
        /// Get All FeedBack By Admin
        /// </summary>
        /// <returns></returns>
        [HttpGet("get")]
        public async Task<IActionResult> GetFeedBack()
        {
            var feedback = await _feedBackService.GetFeedBack();
            return CustomResult("Tải dữ liệu thành công", feedback);
        }

        /// <summary>
        /// Get FeedBack Of Bar by Admin
        /// </summary>
        /// <param name="BarId"></param>
        /// <param name="Status"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet("admin")]
        public async Task<IActionResult> GetFeedBackAdmin([FromQuery] Guid? BarId, [FromQuery] bool? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _feedBackService.GetFeedBackAdmin(BarId, Status, PageIndex, PageSize);
            return Ok(new { totalPage = responses.TotalPage, response = responses.responses});
        }

        /// <summary>
        /// Get FeedBack Of Bar by Manager
        /// </summary>
        /// <param name="BarId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet("manager")]
        public async Task<IActionResult> GetFeedBackManager([FromQuery][Required] Guid BarId, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _feedBackService.GetFeedBackManager(BarId, PageIndex, PageSize);
            return CustomResult("Tải dữ liệu thành công", new { totalPage = responses.TotalPage, response = responses.responses });
        }

        /// <summary>
        /// Get FeedBack By ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedBackByID(Guid id)
        {
            var feedback = await _feedBackService.GetFeedBackByID(id);
            return CustomResult("Tải dữ liệu thành công", feedback);
        }

        /// <summary>
        /// Get FeedBack By BookingID
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetFeedBackByBookingID(Guid bookingId)
        {
            var feedback = await _feedBackService.GetFeedBackByBookingId(bookingId);
            return CustomResult("Tải dữ liệu thành công", feedback);
        }

        /// <summary>
        /// Create FeedBack
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("createFeedBack")]
        public async Task<IActionResult> CreateFeedBack(CreateFeedBackRequest request)
        {
            var feedback = await _feedBackService.CreateFeedBack(request);
            return CustomResult("Tạo Feedback thành công.", feedback);
        }

        /// <summary>
        /// Update FeedBack
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateFeedBack(Guid id, UpdateFeedBackRequest request)
        {
            var feedback = await _feedBackService.UpdateFeedBack(id, request);
            return CustomResult("Cập Nhật Feedback thành công.", feedback);
        }

        /// <summary>
        /// Update FeedBack based FeedbackId
        /// </summary>
        /// <param name="FeedbackId"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateFeedBack([FromQuery] Guid FeedbackId, [FromQuery] bool Status)
        {
            await _feedBackService.UpdateFeedBackByAdmin(FeedbackId, Status);
            return CustomResult("Cập Nhật Feedback thành công.");
        }

        /// <summary>
        /// Delete Update FeedBack (upd sts IsDelete true)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("deleteEmotionCategory/{id}")]
        public async Task<IActionResult> DeleteUpdateFeedBack(Guid id)
        {
            var feedback = await _feedBackService.DeleteUpdateFeedBack(id);
            return CustomResult("Xóa Feedback thành công.", feedback);
        }
    }
}
