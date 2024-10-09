using Application.DTOs.TableType;
using Application.IService;
using CoreApiResponse;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tableTypes = await _tableTypeService.GetAll();
            return CustomResult("Dữ liệu đã tải lên", tableTypes);
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAllForAdmin([FromQuery][Required] int Status)
        {
            var tableTypes = await _tableTypeService.GetAllForAdmin(Status);
            return CustomResult("Dữ liệu đã tải lên", tableTypes);
        }

        [HttpGet("{TableTypeId}")]
        public async Task<IActionResult> GetById(Guid TableTypeId)
        {
            var tableType = await _tableTypeService.GetById(TableTypeId);
            return CustomResult("Dữ liệu đã tải lên", tableType);
        }

        [HttpPost]
        public async Task<IActionResult> Post(TableTypeRequest request)
        {
            await _tableTypeService.CreateTableType(request);
            return CustomResult("Tạo thành công");
        }

        [HttpPatch("{TableTypeId}")]
        public async Task<IActionResult> Put([FromBody] TableTypeRequest request, Guid TableTypeId)
        {
            await _tableTypeService.UpdateTableType(request, TableTypeId);
            return CustomResult("Cập nhật thành công");
        }

        [HttpDelete("{TableTypeId}")]
        public async Task<IActionResult> Delete(Guid TableTypeId)
        {
            var failedNum = await _tableTypeService.DeleteTableType(TableTypeId);
            if (!failedNum)
            {
                return StatusCode(202, "Vẫn còn bàn đang hoạt động thuộc loại bàn này, vui lòng cập nhật lại tất cả bàn của các chi nhánh trước khi xóa loại bàn này");
            }
            return CustomResult("Cập nhật thành công");
        }
    }
}
