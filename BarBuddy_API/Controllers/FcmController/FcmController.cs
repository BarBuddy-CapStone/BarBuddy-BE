using Application.DTOs.Fcm;
using Application.Interfaces;
using CoreApiResponse;
using Domain.CustomException;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Net.Http;

namespace BarBuddy_API.Controllers.FcmController
{
    [ApiController]
    [Route("api/[controller]")]
    public class FcmController : BaseController
    {
        private readonly IFcmService _fcmService;
        private readonly IAuthentication _authentication;

        public FcmController(IFcmService fcmService, IAuthentication authentication)
        {
            _fcmService = fcmService;
            _authentication = authentication;
        }

        [HttpPost("notification")]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                var notificationId = await _fcmService.CreateAndSendNotification(request);
                return CustomResult("Gửi thông báo thành công", notificationId);
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1)
        {
            try 
            {
                var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
                var notifications = await _fcmService.GetNotifications(accountId, page);
                return CustomResult("Lấy danh sách thông báo thành công", notifications);
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadNotificationCount()
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
                var count = await _fcmService.GetUnreadNotificationCount(accountId);
                return CustomResult("Lấy số lượng thông báo chưa đọc thành công", count);
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPatch("notification/{notificationId}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid notificationId)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
                await _fcmService.MarkAsRead(accountId, notificationId);
                return CustomResult("Đánh dấu thông báo đã đọc thành công");
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPatch("notifications/mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
                await _fcmService.MarkAllAsRead(accountId);
                return CustomResult("Đánh dấu tất cả thông báo đã đọc thành công");
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("device-token")]
        public async Task<IActionResult> SaveDeviceToken([FromBody] SaveGuestDeviceTokenRequest request)
        {
            try
            {
                await _fcmService.SaveDeviceToken(request.DeviceToken, request.Platform);
                return CustomResult("Đăng ký device token thành công");
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPatch("device-token/link")]
        public async Task<IActionResult> LinkDeviceTokenToAccount([FromBody] UpdateDeviceTokenRequest request)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
                await _fcmService.LinkDeviceTokenToAccount(accountId, request.DeviceToken);
                return CustomResult("Liên kết device token với tài khoản thành công");
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPatch("device-token/unlink")]
        public async Task<IActionResult> UnlinkDeviceToken([FromBody] UpdateDeviceTokenRequest request)
        {
            try
            {
                await _fcmService.UnlinkDeviceToken(request.DeviceToken);
                return CustomResult("Hủy liên kết device token thành công");
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
