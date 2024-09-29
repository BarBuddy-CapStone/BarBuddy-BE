using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.EmotionCategoryRequest
{
    public class UpdateEmotionCategoryRequest
    {
        [Required(ErrorMessage = "Category name cannot be empty")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Category name must be between 3 and 20 characters")]
        public string CategoryName { get; set; }

    }
}
