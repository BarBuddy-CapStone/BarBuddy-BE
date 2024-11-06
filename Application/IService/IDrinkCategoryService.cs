using Application.DTOs.DrinkCategory;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IDrinkCategoryService
    {
        Task<IEnumerable<DrinkCategoryResponse>> GetAllDrinkCategory(ObjectQuery query);
        Task<IEnumerable<DrinkCategoryResponse>> GetAllDrinkCateOfBar(Guid barId);
        Task<DrinkCategoryResponse> GetDrinkCategoryById(Guid drinkCateId);
        Task<DrinkCategoryResponse> CreateDrinkCategory(DrinkCategoryRequest request);
        Task<DrinkCategoryResponse> UpdateDrinkCategory(Guid drinkCateId, UpdDrinkCategoryRequest request);
        Task<bool> DeleteDrinkCategory(Guid drinkCateId);
    }
}
