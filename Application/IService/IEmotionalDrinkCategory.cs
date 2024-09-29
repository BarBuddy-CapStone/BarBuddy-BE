using Application.DTOs.Response.EmotionCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IEmotionalDrinkCategory
    {
        Task<IEnumerable<EmotionCategoryResponse>> GetEmotionCategory();
    }
}
