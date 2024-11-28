using Application.DTOs.Drink;
using Application.DTOs.DrinkRecommendation;
using Domain.Entities;

namespace Application.IService
{
    public interface IDrinkRecommendationService
    {
        Task<List<DrinkResponse>> GetDrinkRecommendationsBaseOnFeedback(string emotion, int count = 5);
        Task<(List<DrinkResponse>, string)> GetDrinkRecommendations(string emotion, Guid barId);
        Task TrainModel(string trainingFilePath = null);
        Task<List<DrinkRecommendation>> GetRecommendationsAsync(string emotionText, Guid barId);
    }
}