using Application.DTOs.TableType;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.TableType
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableTypeController : BaseController
    {
        private readonly ITableTypeService _tableTypeService;
        public TableTypeController(ITableTypeService tableTypeService)
        {
            _tableTypeService = tableTypeService;
        }
        /// <summary>
        /// Get All TableType
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tableTypes = await _tableTypeService.GetAll();
            return CustomResult("Dữ liệu đã tải lên", tableTypes);
        }

        /// <summary>
        /// Get All Table For Admin
        /// </summary>
        /// <param name="Status"></param>
        /// <returns></returns>
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllForAdmin([FromQuery][Required] int Status)
        {
            var tableTypes = await _tableTypeService.GetAllForAdmin(Status);
            return CustomResult("Dữ liệu đã tải lên", tableTypes);
        }

        /// <summary>
        /// Get Table By TableTypeId
        /// </summary>
        /// <param name="TableTypeId"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpGet("{TableTypeId}")]
        public async Task<IActionResult> GetById(Guid TableTypeId)
        {
            var tableType = await _tableTypeService.GetById(TableTypeId);
            return CustomResult("Dữ liệu đã tải lên", tableType);
        }

        /// <summary>
        ///  Create TableType
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpPost]
        public async Task<IActionResult> Post(TableTypeRequest request)
        {
            try
            {
                await _tableTypeService.CreateTableType(request);
                return CustomResult("Tạo thành công");
            }
            catch (CustomException.UnAuthorizedException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update TableType based TableTypeId
        /// </summary>
        /// <param name="request"></param>
        /// <param name="TableTypeId"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpPatch("{TableTypeId}")]
        public async Task<IActionResult> Put([FromBody] TableTypeRequest request, Guid TableTypeId)
        {
            try
            {
                await _tableTypeService.UpdateTableType(request, TableTypeId);
                return CustomResult("Cập nhật thành công");
            }
            catch (CustomException.UnAuthorizedException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Delete TableType based TableTypeId
        /// </summary>
        /// <param name="TableTypeId"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpDelete("{TableTypeId}")]
        public async Task<IActionResult> Delete(Guid TableTypeId)
        {
            try
            {
                var failedNum = await _tableTypeService.DeleteTableType(TableTypeId);
                if (!failedNum)
                {
                    return StatusCode(202, "Vẫn còn bàn đang hoạt động thuộc loại bàn này, vui lòng cập nhật lại tất cả bàn của các chi nhánh trước khi xóa loại bàn này");
                }
                return CustomResult("Cập nhật thành công");
            }
            catch (CustomException.UnAuthorizedException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get All TT Of Bar
        /// </summary>
        /// <param name="barId"></param>
        /// <returns></returns>
        [Authorize(Roles = "CUSTOMER,MANAGER,STAFF")]
        [HttpGet("getTTOfBar/{barId}")]
        public async Task<IActionResult> GetAllTTOfBar(Guid barId)
        {
            try
            {
                var response = await _tableTypeService.GetAllTTOfBar(barId);
                return CustomResult("Đã tải dữ liệu thành công.", response);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult($"{ex.Message}", System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
