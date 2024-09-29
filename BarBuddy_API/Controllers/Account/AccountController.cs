﻿using Application.DTOs.Account;
using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [AllowAnonymous] // Đổi khi có JWT
        [HttpGet("/api/v1/customer-accounts")]
        public async Task<IActionResult> GetCustomerAccounts([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var accountList = await _accountService.GetPaginationCustomerAccount(pageSize, pageIndex);

            if (accountList == null || !accountList.items.Any())
            {
                return NotFound("No customers found.");
            }
            var result = new
            {
                items = accountList.items,
                count = accountList.count,
                pageIndex = pageIndex,
                pageSize = pageSize
            };
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("/api/v1/staff-accounts")]
        public async Task<IActionResult> GetStaffAccounts([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var accountList = await _accountService.GetPaginationStaffAccount(pageSize, pageIndex);
            var result = new
            {
                items = accountList.items,
                count = accountList.count,
                pageIndex = pageIndex,
                pageSize = pageSize
            };
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("/api/v1/staff-account/detail")]
        public async Task<IActionResult> GetStaffAccountById([FromQuery] string accountId)
        {
            var staffAccount = await _accountService.GetStaffAccountById(accountId);
            return Ok(staffAccount);
        }

        [AllowAnonymous]
        [HttpGet("/api/v1/customer-account/detail")]
        public async Task<IActionResult> GetCustomerAccountByEmail([FromQuery] string accountId)
        {
            var customerAccount = await _accountService.GetCustomerAccountById(accountId);
            return Ok(customerAccount);
        }

        [AllowAnonymous]
        [HttpPut("/api/v1/customer-account")]
        public async Task<IActionResult> UpdateCustomerAccount([FromQuery] string accountId, [FromBody] CustomerAccountRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.UpdateCustomerAccount(accountId, request);
            return Ok(result);
        }
    }
}
