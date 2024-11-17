using Application.DTOs.Fcm;
using Application.Interfaces;
using CoreApiResponse;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var notificationId = await _fcmService.CreateAndSendNotification(request);
            return CustomResult("Gửi thông báo thành công", notificationId);
        }

        [HttpGet("notifications")]
        [Authorize]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1)
        {
            var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
            var notifications = await _fcmService.GetNotificationsForUser(accountId, page);
            return CustomResult("Lấy danh sách thông báo thành công", notifications);
        }

        [HttpGet("notifications/public")]
        public async Task<IActionResult> GetPublicNotifications([FromQuery] string deviceToken, [FromQuery] int page = 1)
        {
            var notifications = await _fcmService.GetPublicNotifications(deviceToken, page);
            return CustomResult("Lấy danh sách thông báo công khai thành công", notifications);
        }

        [HttpPost("sign-device-token")]
        public async Task<IActionResult> SignDeviceToken([FromBody] SaveGuestDeviceTokenRequest request)
        {
            try
            {
                await _fcmService.SaveGuestDeviceToken(
                    request.DeviceToken,
                    request.Platform);
                    
                return CustomResult();
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPatch("update-account-device-token")]
        [Authorize]
        public async Task<IActionResult> UpdateDeviceToken([FromBody] UpdateDeviceTokenRequest request)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
                await _fcmService.UpdateDeviceTokenForUser(accountId, request);
                return CustomResult();
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("notifications/{notificationId}/read")]
        [Authorize]
        public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId)
        {
            var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
            await _fcmService.MarkAsRead(notificationId, accountId);
            return CustomResult("Đánh dấu thông báo đã đọc thành công");
        }

        [HttpPost("notifications/mark-all-read")]
        [Authorize]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var accountId = _authentication.GetUserIdFromHttpContext(HttpContext);
            await _fcmService.MarkAllAsRead(accountId);
            return CustomResult("Đánh dấu tất cả thông báo đã đọc thành công");
        }

        [HttpPost("notifications/{notificationId}/read-by-device")]
        public async Task<IActionResult> MarkNotificationAsReadByDevice(
            Guid notificationId, 
            [FromBody] string deviceToken)
        {
            await _fcmService.MarkAsReadByDeviceToken(notificationId, deviceToken);
            return CustomResult("Đánh dấu thông báo đã đọc thành công");
        }

        [HttpPost("notifications/mark-all-read-by-device")]
        public async Task<IActionResult> MarkAllNotificationsAsReadByDevice([FromBody] string deviceToken)
        {
            await _fcmService.MarkAllAsReadByDeviceToken(deviceToken);
            return CustomResult("Đánh dấu tất cả thông báo đã đọc thành công");
        }
    }
}
