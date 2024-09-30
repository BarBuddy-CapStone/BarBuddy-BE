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
        public string DrinksCategoryName { get; set; }
        public string BarName { get; set; }
        public string DrinkName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public int Status { get; set; }
    }
}
