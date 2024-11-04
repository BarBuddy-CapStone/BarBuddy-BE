using Application.DTOs.Drink;
using Domain.Entities;

namespace Application.IService
{
    public interface IDrinkRecommendationService
    {
        Task<List<DrinkResponse>> GetDrinkRecommendationsBaseOnFeedback(string emotion, int count = 5);
        Task<List<DrinkResponse>> GetDrinkRecommendations(string emotion, Guid barId);
        Task TrainModel(string trainingFilePath = null);
    }
}