using Application.DTOs.Table;
using Application.IService;
using Azure;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.Table
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : BaseController
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }

        /// <summary>
        /// Get All Table For Managing by Manage/Staff
        /// </summary>
        /// <param name="TableTypeId"></param>
        /// <param name="Status"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet("manage")]
        public async Task<IActionResult> GetAll([FromQuery] Guid? TableTypeId, [FromQuery] string? TableName, [FromQuery] int? Status,[FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _tableService.GetAll(TableTypeId, TableName, Status, PageIndex, PageSize);
            return CustomResult("Tải dữ liệu thành công", new { totalPage = responses.TotalPage, response = responses.response });
        }

        /// <summary>
        /// Get All Table Of Bar For Managing by Manage/Staff
        /// </summary>
        /// <param name="BarId"></param>
        /// <param name="TableTypeId"></param>
        /// <param name="Status"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpGet("tables-of-bar")]
        public async Task<IActionResult> GetAllOfBar([FromQuery] Guid BarId, [FromQuery] Guid? TableTypeId, [FromQuery] string? TableName, [FromQuery] int? Status, [FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _tableService.GetAllOfBar(BarId, TableTypeId, TableName, Status, PageIndex, PageSize);
            return CustomResult("Tải dữ liệu thành công", new { totalPage = responses.TotalPage, response = responses.response });
        }

        /// <summary>
        /// Create Table
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateTableRequest request)
        {
            try
            {
                await _tableService.CreateTable(request);
                return CustomResult("Tạo bàn thành công");
            }
            catch (CustomException.DataExistException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update Table based TableId
        /// </summary>
        /// <param name="request"></param>
        /// <param name="TableId"></param>
        /// <returns></returns>
        [HttpPatch("{TableId}")]
        public async Task<IActionResult> Patch([FromBody] UpdateTableRequest request, Guid TableId)
        {
            try
            {
                await _tableService.UpdateTable(TableId, request);
                return CustomResult("Cập nhật bàn thành công");
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
        /// Update Table status based TableId
        /// </summary>
        /// <param name="TableId"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateStatus([FromQuery][Required] Guid TableId ,[FromQuery][Required] int Status)
        {
            try
            {
                await _tableService.UpdateTableStatus(TableId, Status);
                return CustomResult("Cập nhật bàn thành công");
            }
            catch (CustomException.DataNotFoundException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.NotFound);
            }
            catch (CustomException.DataExistException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Delete Table based tableId (change sts IsDelete true)
        /// </summary>
        /// <param name="TableId"></param>
        /// <returns></returns>
        [HttpDelete("{TableId}")]
        public async Task<IActionResult> Delete(Guid TableId)
        {
            try
            {
                var isDeleted = await _tableService.DeleteTable(TableId);
                if (!isDeleted)
                {
                    return CustomResult("Vẫn còn lịch đặt chỗ của bàn này trong tương lai, hãy cập nhật trước khi xóa", System.Net.HttpStatusCode.BadRequest);
                }
                return CustomResult("Xóa bàn thành công");
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
    }
}
