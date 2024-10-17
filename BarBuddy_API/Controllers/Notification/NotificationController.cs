using Application.DTOs.Notification;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("getAllNoti/{accountId}")]
        public async Task<IActionResult> GetNotiOfCusById(Guid accountId)
        {
            try
            {
                var response = await _notificationService.GetAllNotiOfCus(accountId);
                return CustomResult("Data đã tải lên", response);
            }catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }

        [HttpPatch("isRead")]
        public async Task<IActionResult> UpdateIsReadNoti([FromBody] UpdateNotiRequest request)
        {
            try
            {
                var response = await _notificationService.UpdateIsReadNoti(request);
                return CustomResult("Đã đọc hết thông báo", response);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }
    }
}
