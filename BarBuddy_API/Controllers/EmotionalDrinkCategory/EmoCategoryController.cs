using Application.DTOs.Request.EmotionCategoryRequest;
using Application.IService;
using CoreApiResponse;
using Domain.Common;
using Domain.CustomException;
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

        /// <summary>
        /// Get Emotion Category
        /// </summary>
        /// <returns></returns>
        [HttpGet("get")]
        public async Task<IActionResult> GetEmotionCategory([FromQuery] ObjectQueryCustom query)
        {
            var emotional = await _emotionalDrinkCategory.GetEmotionCategory(query);
            return CustomResult("Tải dữ liệu thành công", emotional);
        }

        /// <summary>
        /// Get Emotion Category By ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmotionCategoryByID(Guid id)
        {
            var emotional = await _emotionalDrinkCategory.GetEmotionCategoryByID(id);
            return CustomResult("Tải dữ liệu thành công", emotional);
        }

        /// <summary>
        /// Create Emotion Category
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("createEmotionCategory")]
        public async Task<IActionResult> CreateEmotionCategory(CreateEmotionCategoryRequest request)
        {
            var emotional = await _emotionalDrinkCategory.CreateEmotionCategory(request);
            return CustomResult("Thêm Danh mục cảm xúc thành công.", emotional);
        }

        /// <summary>
        /// Update Emotion Category based EmoCate Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateEmotionCategory(Guid id, UpdateEmotionCategoryRequest request)
        {
            var emotional = await _emotionalDrinkCategory.UpdateEmotionCategory(id, request);
            return CustomResult("Cập Nhật Danh mục cảm xúc thành công.", emotional);
        }

        /// <summary>
        /// Delete Emotion Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("deleteEmotionCategory/{id}")]
        public async Task<IActionResult> DeleteEmotionCategory(Guid id)
        {
            await _emotionalDrinkCategory.DeleteEmotionCategory(id);
            return CustomResult("Xóa Danh mục cảm xúc thành công.");
        }
        /// <summary>
        /// Get Emotion Category Of Bar
        /// </summary>
        /// <param name="barId"></param>
        /// <returns></returns>
        /// <exception cref="CustomException.DataNotFoundException"></exception>
        /// <exception cref="CustomException.InternalServerErrorException"></exception>
        [HttpGet("getEmoCateOfBar")]
        public async Task<IActionResult> GetEmotionCategoryOfBar(Guid barId)
        {
            try
            {
                var emotional = await _emotionalDrinkCategory.GetEmotionCategoryOfBar(barId);
                return CustomResult("Tải dữ liệu thành công", emotional);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
