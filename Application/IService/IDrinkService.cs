using Application.DTOs.Drink;
using Domain.Common;
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
        Task<IEnumerable<DrinkResponse>> GetDrinkCustomerOfBar(Guid barId);
        Task<DrinkResponse> GetDrink(Guid drinkId);
        Task<DrinkResponse> CreateDrink(DrinkRequest request);
        Task<DrinkResponse> UpdateDrink(Guid drinkId,DrinkRequest request);
        Task<PagingDrinkResponse> GetAllDrinkBasedCateId(Guid cateId, ObjectQueryCustom query);
        Task<IEnumerable<DrinkResponse>> GetAllDrinkBasedEmoId(Guid emoId);

        Task<string> CrawlDrink();
        //Task ExportDrinkDataToCsv();
    }
}
