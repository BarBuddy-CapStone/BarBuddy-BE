using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response.EmotionCategory
{
    public class EmotionCategoryResponse
    {
        public Guid BarId { get; set; }
        public Guid EmotionalDrinksCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}
