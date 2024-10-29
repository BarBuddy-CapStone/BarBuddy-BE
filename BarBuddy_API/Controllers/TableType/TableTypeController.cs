using Application.DTOs.TableType;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
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
        [HttpPost]
        public async Task<IActionResult> Post(TableTypeRequest request)
        {
            await _tableTypeService.CreateTableType(request);
            return CustomResult("Tạo thành công");
        }

        /// <summary>
        /// Update TableType based TableTypeId
        /// </summary>
        /// <param name="request"></param>
        /// <param name="TableTypeId"></param>
        /// <returns></returns>
        [HttpPatch("{TableTypeId}")]
        public async Task<IActionResult> Put([FromBody] TableTypeRequest request, Guid TableTypeId)
        {
            await _tableTypeService.UpdateTableType(request, TableTypeId);
            return CustomResult("Cập nhật thành công");
        }

        /// <summary>
        /// Delete TableType based TableTypeId
        /// </summary>
        /// <param name="TableTypeId"></param>
        /// <returns></returns>
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
        [HttpGet("getTTOfBar/{barId}")]
        public async Task<IActionResult> GetAllTTOfBar(Guid barId)
        {
            try
            {
                var response  = await _tableTypeService.GetAllTTOfBar(barId);
                return CustomResult("Đã tải dữ liệu thành công.", response);
            }catch(CustomException.InternalServerErrorException ex)
            {
                return CustomResult($"{ex.Message}", System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
