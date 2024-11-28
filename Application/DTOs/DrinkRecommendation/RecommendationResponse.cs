using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DrinkRecommendation
{
    public class RecommendationResponse
    {
        public List<RecommendationItem> DrinkRecommendation { get; set; }
    }
}
