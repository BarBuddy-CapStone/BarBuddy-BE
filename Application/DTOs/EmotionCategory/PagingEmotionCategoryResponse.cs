using Application.DTOs.Response.EmotionCategory;
using Domain.Common;


namespace Application.DTOs.EmotionCategory
{
    public class PagingEmotionCategoryResponse : PagingResponse
    {
        public List<EmotionCategoryResponse> EmotionCategoryResponses { get; set; }
    }
}
