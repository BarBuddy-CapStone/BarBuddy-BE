using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DrinkCategory
{
    public class DrinkCategoryResponse
    {
        public string DrinksCategoryId { get; set; }
        public string DrinksCategoryName { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    }
}
