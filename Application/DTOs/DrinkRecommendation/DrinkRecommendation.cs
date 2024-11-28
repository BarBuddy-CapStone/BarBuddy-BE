using Application.DTOs.Drink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DrinkRecommendation
{
    public class DrinkRecommendation
    {
        public DrinkResponse Drink { get; set; }
        public string Reason { get; set; }
    }
}
