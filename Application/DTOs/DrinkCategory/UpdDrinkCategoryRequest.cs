using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DrinkCategory
{
    public class UpdDrinkCategoryRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 7, ErrorMessage = "Tên thể loại đồ uống phải từ 7 đến 200 kí tự !")]
        public string Description { get; set; }
    }
}
