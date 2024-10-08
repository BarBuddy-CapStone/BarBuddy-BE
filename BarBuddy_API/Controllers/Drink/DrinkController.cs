using Application.DTOs.Drink;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.Drink
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DrinkController : BaseController
    {
        private readonly IDrinkService _drinkService;

        public DrinkController(IDrinkService drinkService)
        {
            _drinkService = drinkService;
        }
        //[Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetAllDrink()
        {
            try
            {
                var response = await _drinkService.GetAllDrink();
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

        [HttpGet("customer")]
        public async Task<IActionResult> GetAllDrinkCustomer()
        {
            try
            {
                var response = await _drinkService.GetAllDrinkCustomer();
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
        [HttpGet("{drinkId}")]
        public async Task<IActionResult> GetDrink(Guid drinkId)
        {
            try
            {
                var response = await _drinkService.GetDrink(drinkId);
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
        [HttpGet("getDrinkBaedCate/{cateId}")]
        public async Task<IActionResult> GetAllDrinkBasedCateId(Guid cateId)
        {
            try
            {
                var response = await _drinkService.GetAllDrinkBasedCateId(cateId);
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
        [HttpGet("getDrinkBaedEmo/{emoId}")]
        public async Task<IActionResult> GetAllDrinkBasedEmoId(Guid emoId)
        {
            try
            {
                var response = await _drinkService.GetAllDrinkBasedEmoId(emoId);
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

        [HttpPost("/addDrink")]
        public async Task<IActionResult> CreateDrink([FromForm] DrinkRequest request)
        {
            try
            {
                var response = await _drinkService.CreateDrink(request);
                return CustomResult("Created Successfully", response);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [HttpPatch("/updateDrink/{drinkId}")]
        public async Task<IActionResult> UpdateDrink(Guid drinkId, [FromForm] DrinkRequest request)
        {
            try
            {
                var response = await _drinkService.UpdateDrink(drinkId, request);
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
    }
}
