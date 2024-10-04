using Application.DTOs.DrinkCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IDrinkCategoryService
    {
        Task<IEnumerable<DrinkCategoryResponse>> GetAllDrinkCategory();
        Task<DrinkCategoryResponse> GetDrinkCategoryById(Guid drinkCateId);
        Task<DrinkCategoryResponse> CreateDrinkCategory(DrinkCategoryRequest request);
        Task<DrinkCategoryResponse> UpdateDrinkCategory(Guid drinkCateId, DrinkCategoryRequest request);
        Task<bool> DeleteDrinkCategory(Guid drinkCateId);
    }
}
