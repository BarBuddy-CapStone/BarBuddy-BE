using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Account
{
    public class CustomerAccountRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Tên đầy đủ không được để trống và không vượt quá 100 ký tự.")]
        public string Fullname { get; set; }

        [RegularExpression(@"^0[3-9]\d{8,9}$", ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam hợp lệ.")]
        public string Phone { get; set; }

        [Required]
        [CustomValidation(typeof(CustomerAccountRequest), "ValidateAge", ErrorMessage = "Bạn phải đủ 18 tuổi.")]
        public DateTimeOffset Dob { get; set; }

        public static ValidationResult? ValidateAge(DateTimeOffset birthdate, ValidationContext context)
        {
            // Lấy giá trị hiện tại theo DateTimeOffset
            DateTimeOffset today = DateTimeOffset.Now;

            // Tính toán tuổi
            int age = today.Year - birthdate.Year;

            // Kiểm tra xem sinh nhật trong năm nay đã qua chưa
            if (birthdate > today.AddYears(-age))
            {
                age--;
            }

            // Kiểm tra tuổi có lớn hơn hoặc bằng 18 không
            return age >= 18 ? ValidationResult.Success : new ValidationResult("Bạn phải đủ 18 tuổi.");
        }

        public string? Image { get; set; }
        public int Status { get; set; }
    }
}
