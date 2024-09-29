using Application.DTOs.Request.EmotionCategoryRequest;
using Application.IService;
using CoreApiResponse;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.EmotionalDrinkCategory
{
    [Route("api/emocategory")]
    [ApiController]

    public class EmoCategoryController : BaseController
    {
        private readonly IEmotionalDrinkCategoryService _emotionalDrinkCategory;

        public EmoCategoryController(IEmotionalDrinkCategoryService emotionalDrinkCategory)
        {
            _emotionalDrinkCategory = emotionalDrinkCategory;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetEmotionCategory()
        {
            var emotional = _emotionalDrinkCategory.GetEmotionCategory();
            return CustomResult("Tải dữ liệu thành công", emotional);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmotionCategoryByID(Guid id)
        {
            var emotional = _emotionalDrinkCategory.GetEmotionCategoryByID(id);
            return CustomResult("Tải dữ liệu thành công", emotional);
        }

        [HttpPost("createEmotionCategory")]
        public async Task<IActionResult> CreateEmotionCategory(CreateEmotionCategoryRequest request)
        {
            var emotional = await _emotionalDrinkCategory.CreateEmotionCategory(request);
            return CustomResult("Tạo EmotionCategory thành công.", emotional);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateEmotionCategory(Guid id, UpdateEmotionCategoryRequest request)
        {
            var emotional = await _emotionalDrinkCategory.UpdateEmotionCategory(id, request);
            return CustomResult("Cập Nhật EmotionCategory thành công.", emotional);
        }

        [HttpDelete("deleteEmotionCategory/{id}")]
        public async Task<IActionResult> DeleteEmotionCategory(Guid id)
        {
            var emotional = await _emotionalDrinkCategory.DeleteEmotionCategory(id);
            return CustomResult("Xóa EmotionCategory thành công.", emotional);
        }
    }
}
