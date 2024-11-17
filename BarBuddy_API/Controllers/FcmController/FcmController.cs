using Application.DTOs.Fcm;
using Application.Interfaces;
using CoreApiResponse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.FcmController
{
    [ApiController]
    [Route("api/[controller]")]
    public class FcmController : BaseController
    {
        private readonly IFcmService _fcmService;

        public FcmController(IFcmService fcmService)
        {
            _fcmService = fcmService;
        }

        [HttpPost("device-token")]
        public async Task<IActionResult> SaveDeviceToken([FromBody] SaveDeviceTokenRequest request)
        {
            await _fcmService.SaveUserDeviceToken(
                request.AccountId,
                request.DeviceToken,
                request.Platform);

            return CustomResult("Lưu device token thành công");
        }

        [HttpPost("test-notification")]
        public async Task<IActionResult> TestNotification([FromBody] TestNotificationRequest request)
        {
            try
            {
                await _fcmService.SendNotificationToUser(
                    request.AccountId,
                    "Test Notification",
                    "Đây là thông báo test từ BarBuddy!",
                    new Dictionary<string, string> 
                    { 
                        { "type", "test" },
                        { "timestamp", DateTimeOffset.UtcNow.ToString() }
                    }
                );
                
                return CustomResult("Gửi thông báo test thành công!");
            }
            catch (Exception ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
