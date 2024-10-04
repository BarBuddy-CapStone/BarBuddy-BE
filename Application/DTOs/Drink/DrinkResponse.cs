using Application.DTOs.DrinkCategory;
using Application.DTOs.Response.EmotionCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Drink
{
    public class DrinkResponse
    {
        public Guid DrinkId { get; set; }
        public DrinkCategoryResponse DrinkCategoryResponse { get; set; }
        public string BarName { get; set; }
        public string DrinkName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Images { get; set; }
        public List<EmotionCategoryResponse> EmotionsDrink { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool Status { get; set; }
    }
}
