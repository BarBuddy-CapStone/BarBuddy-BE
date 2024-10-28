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
        [Required(ErrorMessage = "Không thể thiếu barId !")]
        public Guid BarId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tên thể loại đồ uống không được trống !")]
        [StringLength(50, MinimumLength =7, ErrorMessage = "Tên thể loại đồ uống phải từ 7 đến 50 kí tự !")]
        public string DrinksCategoryName { get; set; }
        [Required]
        [StringLength(200, MinimumLength = 7, ErrorMessage = "Tên thể loại đồ uống phải từ 7 đến 200 kí tự !")]
        public string Description { get; set; }
    }
}
