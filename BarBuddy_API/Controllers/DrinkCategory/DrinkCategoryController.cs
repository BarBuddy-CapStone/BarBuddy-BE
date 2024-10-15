using Application.DTOs.DrinkCategory;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.DrinkCategory
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DrinkCategoryController : BaseController
    {
        private readonly IDrinkCategoryService _drinkCategoryService;

        public DrinkCategoryController(IDrinkCategoryService drinkCategoryService)
        {
            _drinkCategoryService = drinkCategoryService;
        }

        /// <summary>
        /// Get All Drink Cate
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllDrinkCate()
        {
            try
            {
                var response = await _drinkCategoryService.GetAllDrinkCategory();
                return CustomResult("Data loaded", response);
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
        /// GetAllDrinkCate By cateDrinkId
        /// </summary>
        /// <param name="cateDrinkId"></param>
        /// <returns></returns>
        [HttpGet("{cateDrinkId}")]
        public async Task<IActionResult> GetAllDrinkCate(Guid cateDrinkId)
        {
            try
            {
                var response = await _drinkCategoryService.GetDrinkCategoryById(cateDrinkId);
                return CustomResult("Data loaded", response);
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
        /// Create Drink Category
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("/addCateDrink")]
        public async Task<IActionResult> CreateDrinkCategory([FromBody] DrinkCategoryRequest request)
        {
            try
            {
                var response = await _drinkCategoryService.CreateDrinkCategory(request);
                return CustomResult("Created Successfully", response);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update Drink Category based cateDrinkId
        /// </summary>
        /// <param name="cateDrinkId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPatch("/updateCateDrink/{cateDrinkId}")]
        public async Task<IActionResult> UpdateDrinkCategory(Guid cateDrinkId, [FromBody] DrinkCategoryRequest request)
        {
            try
            {
                var response = await _drinkCategoryService.UpdateDrinkCategory(cateDrinkId, request);
                return CustomResult("Updated Successfully", response);
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
        /// Delete Drink Category (Change Status is False)
        /// </summary>
        /// <param name="cateDrinkId"></param>
        /// <returns></returns>
        [HttpPatch("/deleteCateDrink/{cateDrinkId}")]
        public async Task<IActionResult> DeleteDrinkCategory(Guid cateDrinkId)
        {
            try
            {
                var response = await _drinkCategoryService.DeleteDrinkCategory(cateDrinkId);
                return CustomResult("Deleted Successfully", response);
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
