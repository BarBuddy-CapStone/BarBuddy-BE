using Application.DTOs.Fcm;
using Application.Interfaces;
using CoreApiResponse;
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
            var notificationId = await _fcmService.CreateAndSendNotification(request);
            return CustomResult("Gửi thông báo thành công", notificationId);
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] string deviceToken,
            [FromQuery] int page = 1)
        {
            try 
            {
                Guid? accountId = _authentication.GetUserIdFromHttpContext(HttpContext);

                var notifications = await _fcmService.GetNotifications(deviceToken, accountId, page);
                return CustomResult("Lấy danh sách thông báo thành công", notifications);
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
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

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadNotificationCount([FromQuery] string deviceToken)
        {
            try
            {
                Guid? accountId = _authentication.GetUserIdFromHttpContext(HttpContext);

                var count = await _fcmService.GetUnreadNotificationCount(deviceToken, accountId);
                return CustomResult("Lấy số lượng thông báo chưa đọc thành công", count);
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("notification/customer/{customerId}")]
        public async Task<IActionResult> CreateNotificationForCustomer(
            [FromBody] CreateNotificationRequest request,
            [FromRoute] Guid customerId)
        {
            try 
            {
                var notificationId = await _fcmService.CreateAndSendNotificationToCustomer(request, customerId);
                return CustomResult("Gửi thông báo thành công", notificationId);
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
