using Application.DTOs.Account;
using Application.IService;
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
        [Authorize(Roles ="ADMIN")]
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
        /// Get List Staff Accounts For Manager Of Bar
        /// </summary>
        /// <param name="barId"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("/api/v1/staff-accounts/{barId}")]
        public async Task<IActionResult> GetStaffAccounts(Guid? barId,
            [FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            try
            {

                var accountList = await _accountService.GetPaginationStaffAccount(pageSize, pageIndex, barId);
                var result = new
                {
                    items = accountList.items,
                    total = accountList.total,
                    pageIndex = pageIndex,
                    pageSize = pageSize
                };
                return CustomResult(result);

            }
            catch (CustomException.DataNotFoundException ex) {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get List Staff Accounts
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("/api/v1/staff-accounts")]
        public async Task<IActionResult> GetStaffAccounts([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var accountList = await _accountService.GetPaginationStaffAccount(pageSize, pageIndex, null);
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
        /// Get List Manager Accounts
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("/api/v1/manager-accounts")]
        public async Task<IActionResult> GetManagerAccounts([FromQuery] int pageSize, [FromQuery] int pageIndex)
        {
            var accountList = await _accountService.GetPaginationManagerAccount(pageSize, pageIndex);
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
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpGet("/api/v1/staff-account/detail")]
        public async Task<IActionResult> GetStaffAccountById([FromQuery] Guid accountId)
        {
            var staffAccount = await _accountService.GetStaffAccountById(accountId);
            return CustomResult(staffAccount);
        }

        /// <summary>
        ///  Get Manager Account By Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("/api/v1/manager-account/detail")]
        public async Task<IActionResult> GetManagerAccountById([FromQuery] Guid accountId)
        {
            var staffAccount = await _accountService.GetManagerAccountById(accountId);
            return CustomResult(staffAccount);
        }

        /// <summary>
        /// Get Customer Account By Email
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("/api/v1/customer-account/detail")]
        public async Task<IActionResult> GetCustomerAccountByEmail([FromQuery] Guid accountId)
        {
            var customerAccount = await _accountService.GetCustomerAccountById(accountId);
            return CustomResult("Data loaded", customerAccount);
        }

        /// <summary>
        /// Get Customer Account By Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER")]
        [HttpGet("/api/v1/customer/{accountId}")]
        public async Task<IActionResult> GetCustomerAccountById(Guid accountId)
        {
            var customerAccount = await _accountService.GetCustomerInfoById(accountId);
            return CustomResult(customerAccount);
        }

        /// <summary>
        /// Create Staff Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpPost("/api/v1/staff-account")]
        public async Task<IActionResult> CreateStaffAccount([FromBody] StaffAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.CreateStaffAccount(request);
            return CustomResult("Tạo tài khoản thành công!", result);
        }

        /// <summary>
        /// Create Manager Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPost("/api/v1/manager-account")]
        public async Task<IActionResult> CreateManagerAccount([FromBody] ManagerAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.CreateManagerAccount(request);
            return CustomResult("Tạo tài khoản thành công!", result);
        }

        /// <summary>
        /// Create Customer Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPost("/api/v1/customer-account")]
        public async Task<IActionResult> CreateCustomerAccount([FromBody] CustomerAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.CreateCustomerAccount(request);
            return CustomResult("Tạo tài khoản thành công!", result);
        }

        /// <summary>
        /// Update Staff Account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN,MANAGER")]
        [HttpPatch("/api/v1/staff-account")]
        public async Task<IActionResult> UpdateStaffAccount([FromQuery] Guid accountId, [FromBody] StaffAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.UpdateStaffAccount(accountId, request);
            return CustomResult("Thông tin đã được cập nhật thành công!", result);
        }

        /// <summary>
        /// Update Manager Account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("/api/v1/manager-account")]
        public async Task<IActionResult> UpdateManagerAccount([FromQuery] Guid accountId, [FromBody] ManagerAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.UpdateManagerAccount(accountId, request);
            return CustomResult("Thông tin đã được cập nhật thành công!", result);
        }

        /// <summary>
        /// Update Customer Account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "ADMIN,CUSTOMER")]
        [HttpPatch("/api/v1/customer-account")]
        public async Task<IActionResult> UpdateCustomerAccount([FromQuery] Guid accountId, [FromBody] CustomerAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _accountService.UpdateCustomerAccount(accountId, request);
            return CustomResult("Thông tin đã được cập nhật thành công!", result);
        }

        /// <summary>
        /// Update Customer Account By Customer
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER")]
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
        [Authorize(Roles = "CUSTOMER")]
        [HttpPatch("/api/v1/customer/avatar/{accountId}")]
        public async Task<IActionResult> UpdateCustomerAvatar(Guid accountId, [FromForm] IFormFile Image)
        {
            var res = await _accountService.UpdateCustomerAvatar(accountId, Image);
            return CustomResult(new { url = res });
        }

        [HttpPatch("/api/v1/customer/updSts")]
        public async Task<IActionResult> UpdateCustomeStatus(UpdCustomerStsRequest request)
        {
            try
            {
                var response = await _accountService.UpdateCustomerSts(request);
                return CustomResult("Đã thay đổi trạng thái thành công !", response);
            }catch(CustomException.DataNotFoundException ex)
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
