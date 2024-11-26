using Application.DTOs.Notification;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BarBuddy_API.Controllers.Notification
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("getAllNoti/{accountId}")]
        public async Task<IActionResult> GetNotiOfCusById(Guid accountId)
        {
            try
            {
                var response = await _notificationService.GetAllNotiOfCus(accountId);
                return CustomResult("Data đã tải lên", response);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
        [Authorize(Roles = "CUSTOMER")]
        [HttpPatch("isRead")]
        public async Task<IActionResult> UpdateIsReadNoti([FromBody] UpdateNotiRequest request)
        {
            try
            {
                var response = await _notificationService.UpdateIsReadNoti(request);
                return CustomResult("Đã đọc hết thông báo", response);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
