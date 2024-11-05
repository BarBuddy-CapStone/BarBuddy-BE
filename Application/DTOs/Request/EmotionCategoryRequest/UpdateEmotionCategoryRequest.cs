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
        [Required(ErrorMessage = "Tên thể loại danh mục cảm xúc không thể thiếu !")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Thể loại cảm xúc phải có độ dài từ 3 đến 20 kí tự !")]
        public string CategoryName { get; set; }
        [Required(ErrorMessage = "Mô tả không được để trống !")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Thể loại cảm xúc phải có độ dài từ 3 đến 50 kí tự !")]
        public string Description { get; set; }

    }
}
