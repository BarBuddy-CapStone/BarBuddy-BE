using Application.DTOs.Table;
using Application.IService;
using CoreApiResponse;
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
        public async Task<IActionResult> GetAllForBar([FromQuery][Required] Guid TableTypeId, [FromQuery] int? Status,[FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _tableService.GetAll(TableTypeId, Status, PageIndex, PageSize);
            return Ok(new { TableTypeId = responses.TableTypeId, TableTypeName = responses.TableTypeName, totalPage = responses.TotalPage, response = responses.response });
        }

        /// <summary>
        /// Create Table
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateTableRequest request)
        {
            await _tableService.CreateTable(request);
            return Ok("Tạo bàn thành công");
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
            await _tableService.UpdateTable(TableId, request);
            return Ok("Cập nhật bàn thành công");
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
            await _tableService.UpdateTableStatus(TableId, Status);
            return CustomResult("Cập nhật bàn thành công");
        }

        /// <summary>
        /// Delete Table based tableId (change sts IsDelete true)
        /// </summary>
        /// <param name="TableId"></param>
        /// <returns></returns>
        [HttpDelete("{TableId}")]
        public async Task<IActionResult> Delete(Guid TableId)
        {
            var isDeleted = await _tableService.DeleteTable(TableId);
            if (!isDeleted)
            {
                return StatusCode(202, "Vẫn còn lịch đặt chỗ của bàn này trong tương lai, hãy cập nhật trước khi xóa");
            }
            return Ok("Xóa bàn thành công");
        }
    }
}
