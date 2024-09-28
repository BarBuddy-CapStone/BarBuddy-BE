using Application.DTOs.TableTypeDto;
using Application.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableTypeController : ControllerBase
    {
        private readonly ITableTypeService _tableTypeService;
        public TableTypeController(ITableTypeService tableTypeService)
        {
            _tableTypeService = tableTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var tableTypes = await _tableTypeService.GetAll();
                return Ok(tableTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{TableTypeId}")]
        public async Task<IActionResult> GetById(string TableTypeId)
        {
            try
            {
                var tableType = await _tableTypeService.GetById(TableTypeId);
                if(tableType == null)
                {
                    return NotFound("Không tìm thấy loại bàn");
                }
                return Ok(tableType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(TableTypeDtoRequest request)
        {
            try
            {
                if (request.MaximumGuest < request.MinimumGuest) {
                    return BadRequest("Số lượng đa tối thiểu phải bé hơn số lượng khách tối đa");
                }
                await _tableTypeService.CreateTableType(request);
                return Ok("Tạo thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{TableTypeId}")]
        public async Task<IActionResult> Put([FromBody] TableTypeDtoRequest request, string TableTypeId)
        {
            try
            {
                Console.WriteLine("1");
                if (request.MaximumGuest < request.MinimumGuest)
                {
                    return BadRequest("Số lượng đa tối thiểu phải bé hơn số lượng khách tối đa");
                }
                var isUpdated = await _tableTypeService.UpdateTableType(request, TableTypeId);
                if (!isUpdated) { 
                    return NotFound("Loại bàn không tồn tại");
                }
                return Ok("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{TableTypeId}")]
        public async Task<IActionResult> Delete(string TableTypeId)
        {
            try
            {
                var failedNum = await _tableTypeService.DeleteTableType(TableTypeId);
                if (failedNum == 1)
                {
                    return NotFound("Loại bàn không tồn tại");
                }
                if (failedNum == 2) {
                    return StatusCode(202, "Vẫn còn bàn đang hoạt động thuộc loại bàn này, vui lòng cập nhật lại tất cả bàn của các chi nhánh trước khi xóa loại bàn này");
                }
                return Ok("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
