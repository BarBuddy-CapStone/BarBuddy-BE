using Application.DTOs.Event;
using Application.DTOs.Events;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BarBuddy_API.Controllers.Event
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : BaseController
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllEvent([FromQuery] EventQuery query)
        {
            try
            {
                var response = await _eventService.GetAllEvent(query);
                return CustomResult("Đã tải dữ liệu thành công", response);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("getOne/{eventId}")]
        public async Task<IActionResult> GetOneEvent(Guid eventId)
        {
            try
            {
                var response = await _eventService.GetOneEvent(eventId);
                return CustomResult("Đã tải dữ liệu thành công", response);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("createEvent")]
        public async Task<IActionResult> CreateEvent([FromBody] EventRequest request)
        {
            try
            {
                await _eventService.CreateEvent(request);
                return CustomResult("Đã tạo thành công");
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPatch("updateEvent")]
        public async Task<IActionResult> UpdateEvent([FromQuery] Guid eventId, [FromBody] UpdateEventRequest request)
        {
            try
            {
                await _eventService.UpdateEvent(eventId, request);
                return CustomResult("Đã update thành công");
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
