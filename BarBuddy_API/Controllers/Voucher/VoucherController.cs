﻿using Application.DTOs.Events.EventVoucher;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.Voucher
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : BaseController
    {
        private readonly IEventVoucherService _eventVoucherService;

        public VoucherController(IEventVoucherService eventVoucherService)
        {
            _eventVoucherService = eventVoucherService;
        }
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("getOneVoucher")]
        public async Task<IActionResult> GetVoucherByCode([FromQuery] VoucherQueryRequest request)
        {
            try
            {
                var response = await _eventVoucherService.GetVoucherByCode(request);
                return CustomResult("Đã tải dữ liệu.", response);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
