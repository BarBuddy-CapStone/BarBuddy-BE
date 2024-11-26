using Application.DTOs.Request.FeedBackRequest;
using Application.IService;
using CoreApiResponse;
using Domain.Common;
using Domain.CustomException;
using Microsoft.AspNetCore.Authorization;
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
        //[Authorize]
        //[HttpGet("get")]
        //public async Task<IActionResult> GetFeedBack()
        //{
        //    var feedback = await _feedBackService.GetFeedBack();
        //    return CustomResult("Tải dữ liệu thành công", feedback);
        //}

        /// <summary>
        /// Get FeedBack Of Bar by Admin
        /// </summary>
        /// <param name="BarId"></param>
        /// <param name="Status"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetFeedBackAdmin([FromQuery] Guid? BarId, [FromQuery] bool? Status, [FromQuery] ObjectQueryCustom query)
        {
            var responses = await _feedBackService.GetFeedBackAdmin(BarId, Status, query);
            return CustomResult(new { response = responses });
        }

        /// <summary>
        /// Get FeedBack Of Bar by Manager
        /// </summary>
        /// <param name="BarId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpGet("manager")]
        public async Task<IActionResult> GetFeedBackManager([FromQuery][Required] Guid BarId, [FromQuery] ObjectQueryCustom query)
        {
            try
            {
                var responses = await _feedBackService.GetFeedBackManager(BarId, query);
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
        [Authorize(Roles = "CUSTOMER")]
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
        [Authorize(Roles = "CUSTOMER")]
        [HttpPost("createFeedBack")]
        public async Task<IActionResult> CreateFeedBack(CreateFeedBackRequest request)
        {
            try
            {
                var feedback = await _feedBackService.CreateFeedBack(request);
                return CustomResult("Tạo Feedback thành công.", feedback);
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
        /// Update FeedBack
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Authorize(Roles ="ADMIN")]
        //[HttpPatch("{id}")]
        //public async Task<IActionResult> UpdateFeedBack(Guid id, UpdateFeedBackRequest request)
        //{
        //    var feedback = await _feedBackService.UpdateFeedBack(id, request);
        //    return CustomResult("Cập Nhật Feedback thành công.", feedback);
        //}

        /// <summary>
        /// Update FeedBack based FeedbackId
        /// </summary>
        /// <param name="FeedbackId"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
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
        //[HttpPatch("deleteEmotionCategory/{id}")]
        //public async Task<IActionResult> DeleteUpdateFeedBack(Guid id)
        //{
        //    var feedback = await _feedBackService.DeleteUpdateFeedBack(id);
        //    return CustomResult("Xóa Feedback thành công.", feedback);
        //}
    }
}
