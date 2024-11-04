using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ML
{
    public class DrinkEmotionData
    {
        public string CommentEmotional { get; set; }
        public int Rating { get; set; }
        public string DrinkName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string EmotionCategory { get; set; }
    }
}
