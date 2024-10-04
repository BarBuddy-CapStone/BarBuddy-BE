using Application.DTOs.Bar;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BarBuddy_API.Controllers.Bar
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BarController : BaseController
    {
        private readonly IBarService _barService;

        public BarController(IBarService barService)
        {
            _barService = barService;
        }

        [HttpGet("admin/barmanager")]
        public async Task<IActionResult> GetAllBar()
        {
            try
            {
                var response = await _barService.GetAllBar();
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("/api/v1/bars")]
        public async Task<IActionResult> GetAllBarWithFeedback()
        {
            try
            {
                var response = await _barService.GetAllBarWithFeedback();
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("admin/barProfile/{barId}")]
        public async Task<IActionResult> GetBarById(Guid barId)
        {
            try
            {
                var response = await _barService.GetBarById(barId);
                return CustomResult("Data loaded", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("admin/addBar")]
        public async Task<IActionResult> CreateBar([FromForm] BarRequest request)
        {
            try
            {
                var response = await _barService.CreateBar(request);
                return CustomResult("Created Successfully", response);
            }
            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, HttpStatusCode.BadRequest);
            }

            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut("admin/updateBar/{barId}")]
        public async Task<IActionResult> UpdateBar(Guid barId, [FromForm] BarRequest request)
        {
            try
            {
                var response = await _barService.UpdateBarById(barId, request);
                return CustomResult("Updated Successfully", response);
            }
            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, HttpStatusCode.BadRequest);
            }

            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, HttpStatusCode.NotFound);
            }

            catch (Exception e)
            {
                return CustomResult(e.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
