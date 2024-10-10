using Application.DTOs.Request.FeedBackRequest;
using Application.IService;
using CoreApiResponse;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("get")]
        public async Task<IActionResult> GetFeedBack()
        {
            var feedback = _feedBackService.GetFeedBack();
            return CustomResult("Tải dữ liệu thành công", feedback);
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetFeedBackAdmin([FromQuery] Guid? BarId, [FromQuery] bool? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _feedBackService.GetFeedBackAdmin(BarId, Status, PageIndex, PageSize);
            return Ok(new { totalPage = responses.TotalPage, response = responses.responses});
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedBackByID(Guid id)
        {
            var feedback = _feedBackService.GetFeedBackByID(id);
            return CustomResult("Tải dữ liệu thành công", feedback);
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetFeedBackByBookingID(Guid bookingId)
        {
            var feedback = await _feedBackService.GetFeedBackByBookingId(bookingId);
            return CustomResult("Tải dữ liệu thành công", feedback);
        }

        [HttpPost("createFeedBack")]
        public async Task<IActionResult> CreateFeedBack(CreateFeedBackRequest request)
        {
            var feedback = await _feedBackService.CreateFeedBack(request);
            return CustomResult("Tạo Feedback thành công.", feedback);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateFeedBack(Guid id, UpdateFeedBackRequest request)
        {
            var feedback = await _feedBackService.UpdateFeedBack(id, request);
            return CustomResult("Cập Nhật Feedback thành công.", feedback);
        }
        
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateFeedBack([FromQuery] Guid FeedbackId, [FromQuery] bool Status)
        {
            await _feedBackService.UpdateFeedBackByAdmin(FeedbackId, Status);
            return CustomResult("Cập Nhật Feedback thành công.");
        }

        [HttpPatch("deleteEmotionCategory/{id}")]
        public async Task<IActionResult> DeleteUpdateFeedBack(Guid id)
        {
            var feedback = await _feedBackService.DeleteUpdateFeedBack(id);
            return CustomResult("Xóa Feedback thành công.", feedback);
        }
    }
}
