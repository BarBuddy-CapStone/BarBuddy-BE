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
        /// <summary>
        /// Get All Drink By Admin
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
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

        /// <summary>
        /// Get All Drink Customer diffenrence with Admin is Status = Inactive 
        /// </summary>
        /// <returns></returns>
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
        
        [AllowAnonymous]
        [HttpGet("customer/{barId}")]
        public async Task<IActionResult> GetDrinkCustomer(Guid barId)
        {
            try
            {
                var response = await _drinkService.GetDrinkCustomerOfBar(barId);
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
        /// Get Drink by Id
        /// </summary>
        /// <param name="drinkId"></param>
        /// <returns></returns>
        //[Authorize(Roles = "MANAGER")]
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

        /// <summary>
        /// Get All Drink Based CateId
        /// </summary>
        /// <param name="cateId"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
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

        /// <summary>
        /// Get All Drink Based EmoId
        /// </summary>
        /// <param name="emoId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create Drink By Admin
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
        [HttpPost("/addDrink")]
        public async Task<IActionResult> CreateDrink([FromForm] DrinkRequest request)
        {
            try
            {
                var response = await _drinkService.CreateDrink(request);
                return CustomResult("Created Successfully", response);
            }
            catch (CustomException.UnAuthorizedException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InvalidDataException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Update Drink based DrinkId by Admin
        /// </summary>
        /// <param name="drinkId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Roles = "MANAGER")]
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
            catch (CustomException.UnAuthorizedException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.Unauthorized);
            }
            catch (CustomException.InternalServerErrorException e)
            {
                return CustomResult(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Crawl-Drink Data
        /// </summary>
        /// <returns></returns>
        [HttpGet("/crawl-drink")]
        public async Task<IActionResult> CrawlDrink()
        {
            var response = await _drinkService.CrawlDrink();
            return CustomResult(response);
        }
    }
}
