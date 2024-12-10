using Application.DTOs.BarTime;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class BarBaseRequest
    {
        [Required(ErrorMessage = "Tên quán Bar không thể trống !")]
        [StringLength(50, MinimumLength = 7, ErrorMessage = "Tên quán Bar phải từ 7 đến 50 kí tự !")]
        public string BarName { get; set; }

        [Required(ErrorMessage = "Địa chỉ Bar không thể trống !")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Địa chỉ Bar phải từ 7 đến 100 kí tự !")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Mô tả Bar không thể trống !")]
        [StringLength(500, MinimumLength = 7, ErrorMessage = "Mô tả Bar phải từ 7 đến 500 kí tự !")]
        public string Description { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam hợp lệ !")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Email không thể để trống!")]
        [EmailAddress]
        [StringLength(100, ErrorMessage = "Email tối đa 100 kí tự!")]
        public string Email { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? imgsAsString { get; set; }
        [Range(0,100, ErrorMessage ="Giảm giá phải từ 0 đến 100 !")]
        [Required(ErrorMessage = "Giảm giá chỉ từ 0 đến 100% !")]
        public double Discount { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        [Range(0,24,ErrorMessage ="Thời gian của một slot chỉ từ 0 đến 24 giờ")]
        public double TimeSlot { get; set; }
    }
}
