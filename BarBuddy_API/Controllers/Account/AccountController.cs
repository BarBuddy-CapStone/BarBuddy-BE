using Application.DTOs.Account;
using Application.IService;
using Application.Service;
using Azure.Core;
using CoreApiResponse;
using Domain.CustomException;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BarBuddy_API.Controllers.Account
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        /// <summary>
        /// Get List Customer Accounts
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [AllowAnonymous] // Đổi khi có JWT
        [HttpGet("/api/v1/customer-accounts")]
        public async Task<IActionResult> GetCustomerAccounts([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var accountList = await _accountService.GetPaginationCustomerAccount(pageSize, pageIndex);

            var result = new
            {
                items = accountList.items,
                total = accountList.total,
                pageIndex = pageIndex,
                pageSize = pageSize
            };
            return CustomResult(result);
        }

        /// <summary>
        /// Get List Staff Accounts
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api/v1/staff-accounts")]
        public async Task<IActionResult> GetStaffAccounts([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var accountList = await _accountService.GetPaginationStaffAccount(pageSize, pageIndex);
            var result = new
            {
                items = accountList.items,
                total = accountList.total,
                pageIndex = pageIndex,
                pageSize = pageSize
            };
            return CustomResult(result);
        }

        /// <summary>
        ///  Get Staff Account By Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api/v1/staff-account/detail")]
        public async Task<IActionResult> GetStaffAccountById([FromQuery] Guid accountId)
        {
            var staffAccount = await _accountService.GetStaffAccountById(accountId);
            return CustomResult(staffAccount);
        }

        /// <summary>
        /// Get Customer Account By Email
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("/api/v1/customer-account/detail")]
        public async Task<IActionResult> GetCustomerAccountByEmail([FromQuery] Guid accountId)
        {
            var customerAccount = await _accountService.GetCustomerAccountById(accountId);
            return CustomResult(customerAccount);
        }

        /// <summary>
        /// Get Customer Account By Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [HttpGet("/api/v1/customer/{accountId}")]
        public async Task<IActionResult> GetCustomerAccountById(Guid accountId)
        {
            var customerAccount = await _accountService.GetCustomerInfoById(accountId);
            return Ok(customerAccount);
        }

        /// <summary>
        /// Create Staff Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/v1/staff-account")]
        public async Task<IActionResult> CreateStaffAccount([FromBody] StaffAccountRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.CreateStaffAccount(request);
            return CustomResult(result);
        }

        /// <summary>
        /// Create Customer Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("/api/v1/customer-account")]
        public async Task<IActionResult> CreateCustomerAccount([FromBody] CustomerAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.CreateCustomerAccount(request);
            return CustomResult(result);
        }

        /// <summary>
        /// Update Staff Account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPatch("/api/v1/staff-account")]
        public async Task<IActionResult> UpdateStaffAccount([FromQuery] Guid accountId, [FromBody] StaffAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.UpdateStaffAccount(accountId, request);
            return CustomResult(result);
        }

        /// <summary>
        /// Update Customer Account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPatch("/api/v1/customer-account")]
        public async Task<IActionResult> UpdateCustomerAccount([FromQuery] Guid accountId, [FromBody] CustomerAccountRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.UpdateCustomerAccount(accountId, request);
            return CustomResult(result);
        }

        /// <summary>
        /// Update Customer Account By Customer
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPatch("/api/v1/customer/{accountId}")]
        public async Task<IActionResult> UpdateCustomerAccountByCustomer(Guid accountId, [FromBody] CustomerInfoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _accountService.UpdateCustomerAccountByCustomer(accountId, request);
            return CustomResult("Update thành công");
        }

        /// <summary>
        /// Update Customer Avatar
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="Image"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPatch("/api/v1/customer/avatar/{accountId}")]
        public async Task<IActionResult> UpdateCustomerAvatar(Guid accountId, [FromForm] IFormFile Image)
        {
            var res = await _accountService.UpdateCustomerAvatar(accountId, Image);
            return CustomResult(new {url = res});
        }
    }
}
