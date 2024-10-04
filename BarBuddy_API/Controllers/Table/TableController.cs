using Application.DTOs.Table;
using Application.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarBuddy_API.Controllers.Table
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }

        [HttpGet("staff")]
        public async Task<IActionResult> GetAllForStaff([FromQuery] Guid? BarId, [FromQuery][Required] Guid TableTypeId, [FromQuery] int? Status,[FromQuery] int PageIndex = 1, [FromQuery] int PageSize = 10)
        {
            var responses = await _tableService.GetAll(BarId, TableTypeId, Status, PageIndex, PageSize);
            return Ok(new { TableTypeName = responses.TableTypeName, totalPage = responses.TotalPage, response = responses.response });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateTableRequest request)
        {
            await _tableService.CreateTable(request);
            return Ok("Tạo bàn thành công");
        }

        [HttpPatch("{TableId}")]
        public async Task<IActionResult> Patch([FromBody] UpdateTableRequest request, Guid TableId)
        {
            await _tableService.UpdateTable(TableId, request);
            return Ok("Cập nhật bàn thành công");
        }

        [HttpDelete("{TableId}")]
        public async Task<IActionResult> Delete(Guid TableId)
        {
            await _tableService.DeleteTable(TableId);
            return Ok("Xóa bàn thành công");
        }
    }
}
