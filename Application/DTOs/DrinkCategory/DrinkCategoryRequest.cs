using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DrinkCategory
{
    public class DrinkCategoryRequest
    {
        [Required]
        public Guid BarId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "DrinksCategoryName is required.")]
        public string DrinksCategoryName { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
