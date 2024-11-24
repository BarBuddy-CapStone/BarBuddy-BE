using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DrinkCategory
{
    public class PagingDrinkCategoryResponse : PagingResponse
    {
        public List<DrinkCategoryResponse> DrinkCategoryResponses { get; set; }
    }
}
