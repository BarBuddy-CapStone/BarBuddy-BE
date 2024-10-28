using Application.DTOs.Request.EmotionCategoryRequest;
using Application.DTOs.Response.EmotionCategory;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IEmotionalDrinkCategoryService
    {
        Task<IEnumerable<EmotionCategoryResponse>> GetEmotionCategory();
        Task<EmotionCategoryResponse> GetEmotionCategoryByID(Guid id);
        Task<EmotionCategoryResponse> CreateEmotionCategory(CreateEmotionCategoryRequest request);
        Task<EmotionCategoryResponse> UpdateEmotionCategory(Guid id , UpdateEmotionCategoryRequest request);
        Task DeleteEmotionCategory(Guid id);
        Task<List<EmotionCategoryResponse>> GetEmotionCategoryOfBar(Guid barId);
    }
}
