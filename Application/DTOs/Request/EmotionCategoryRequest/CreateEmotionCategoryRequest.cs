using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.EmotionCategoryRequest
{
    public class CreateEmotionCategoryRequest
    {
        [Required]
        public Guid BarId { get; set; }
        [Required(ErrorMessage = "Tên thể loại danh mục cảm xúc không thể thiếu")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Category name phải có độ dài từ 3 đến 20 kí tự !")]
        public string CategoryName { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
