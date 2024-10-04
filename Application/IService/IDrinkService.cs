using Application.DTOs.Drink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IDrinkService
    {
        Task<IEnumerable<DrinkResponse>> GetAllDrink();
        Task<IEnumerable<DrinkResponse>> GetAllDrinkCustomer();
        Task<DrinkResponse> GetDrink(Guid drinkId);
        Task<DrinkResponse> CreateDrink(DrinkRequest request);
        Task<DrinkResponse> UpdateDrink(Guid drinkId,DrinkRequest request);
        Task<IEnumerable<DrinkResponse>> GetAllDrinkBasedCateId(Guid cateId);
        Task<IEnumerable<DrinkResponse>> GetAllDrinkBasedBarId(Guid barId);
    }
}
