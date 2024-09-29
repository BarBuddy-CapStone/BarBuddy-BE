using Application.DTOs.Request.EmotionCategoryRequest;
using Application.IService;
using CoreApiResponse;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.EmotionalDrinkCategory
{
    [Route("api/emocategory")]
    [ApiController]

    public class EmoCategoryControllerr : BaseController
    {
        private readonly IEmotionalDrinkCategory _emotionalDrinkCategory;

        public EmoCategoryControllerr(IEmotionalDrinkCategory emotionalDrinkCategory)
        {
            _emotionalDrinkCategory = emotionalDrinkCategory;
        }

        [HttpGet("getEmotion")]
        public async Task<IActionResult> GetEmotionCategory()
        {
            var emotional = _emotionalDrinkCategory.GetEmotionCategory();
            return CustomResult("Tải dữ liệu thành công", emotional);
        }

        //[HttpPost("createEmotionCategory")]
        //public async Task<IActionResult> CreateEmotionCategory(CreateEmotionCategoryRequest request)
        //{
        //    var emotional = _emotionalDrinkCategory.CreateEmotionCategory(request);
        //    return CustomResult("Tạo EmotionCategory thành công.", emotional);
        //}
    }
}
