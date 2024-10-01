using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DrinkEmoCate
{
    public class DrinkEmoCateRequest
    {
        public Guid DrinkId { get; set; }
        public Guid EmotionalDrinkCategoryId { get; set; }
    }
}
