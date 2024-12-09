using Application.Common;
using Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Authen
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password không thể trống !")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password phải từ 6 and 20 kí tự")]

        public string Password { get; set; }

        [Required(ErrorMessage = "Password không thể trống !")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password phải từ 6 and 20 kí tự")]
        public string ConfirmPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Tên đầy đủ không được để trống và không vượt quá 100 ký tự.")]
        public string Fullname { get; set; }

        [RegularExpression(@"^0[3-9]\d{8,9}$", ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam hợp lệ.")]
        public string Phone { get; set; }

        [Required]
        //[CustomValidation(typeof(Utils), nameof(Utils.ValidateAge), ErrorMessage = "Bạn phải đủ 18 tuổi.")]
        public DateTimeOffset Dob { get; set; }
    }
}
