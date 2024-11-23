using Application.DTOs.Event;
using Application.DTOs.Events;
using Application.IService;
using CoreApiResponse;
using Domain.Common;
using Domain.CustomException;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
        
        /// <summary>
        /// Get All Event
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
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
        /// <summary>
        /// Get Event By Bar Id
        /// </summary>
        /// <param name="barId"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{barId}")]
        public async Task<IActionResult> GetEventByBarId(Guid barId, [FromQuery] ObjectQuery query)
        {
            try
            {
                var response = await _eventService.GetEventsByBarId(query, barId);
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

        /// <summary>
        /// Get One Event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [AllowAnonymous]
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

        /// <summary>
        /// Create Event
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        
        [HttpPost("createEvent")]
        [Authorize(Roles = "MANAGER")]
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
            catch (CustomException.UnAuthorizedException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.Unauthorized);
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

        /// <summary>
        /// Update Event
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpPatch("updateEvent")]
        public async Task<IActionResult> UpdateEvent([FromQuery][Required] Guid eventId, [FromBody] UpdateEventRequest request)
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

        /// <summary>
        /// Delete Event
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpDelete("deleteEvent")]
        public async Task<IActionResult> DeleteEvent([FromQuery][Required] Guid eventId)
        {
            try
            {
                await _eventService.DeleteEvent(eventId);
                return CustomResult("Đã xóa thành công");
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
    }
}
