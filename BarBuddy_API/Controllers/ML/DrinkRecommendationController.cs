using Application.IService;
using CoreApiResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BarBuddy_API.Controllers.ML
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrinkRecommendationController : BaseController
    {
        private readonly IDrinkRecommendationService _recommendationService;

        public DrinkRecommendationController(IDrinkRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpPost("train")]
        public async Task<IActionResult> TrainModel()
        {
            await _recommendationService.TrainModel("D:\\MachineLearningDocs\\Dataseed\\emotionDataset.txt");
            return Ok("Model đã được huấn luyện thành công");
        }

        //[Authorize(Roles = "CUSTOMER")]
        [HttpGet("drink-recommendation")]
        public async Task<IActionResult> GetRecommendations([FromQuery] string emotion, [FromQuery] Guid barId)
        {
            var recommendations = await _recommendationService.GetDrinkRecommendations(emotion, barId);
            var response = new
            {
                drinkList = recommendations.Item1,
                emotion = recommendations.Item2
            };
            return CustomResult("Đã tải dữ liệu thành công !", response);
        }

        //[Authorize(Roles = "CUSTOMER")]
        [HttpGet("drink-recommendation-v2")]
        public async Task<IActionResult> GetRecommendationsV2([FromQuery] string emotion, [FromQuery] Guid barId)
        {
            var recommendations = await _recommendationService.GetRecommendationsAsync(emotion, barId);
            
            return CustomResult("Đã tải dữ liệu thành công !", recommendations);
        }
    }
}
