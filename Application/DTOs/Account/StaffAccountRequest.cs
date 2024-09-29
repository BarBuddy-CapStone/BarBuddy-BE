using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Account
{
    public class StaffAccountRequest
    {
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Tên đầy đủ không được để trống và không vượt quá 100 ký tự.")]
        public string Fullname { get; set; }

        [RegularExpression(@"^0[3-9]\d{8,9}$", ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam hợp lệ.")]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(CustomerAccountRequest), "ValidateAge", ErrorMessage = "Bạn phải đủ 18 tuổi.")]
        public DateTime Birthdate { get; set; }

        public static ValidationResult? ValidateAge(DateTime birthdate, ValidationContext context)
        {
            int age = DateTime.Today.Year - birthdate.Year;
            if (birthdate > DateTime.Today.AddYears(-age)) age--;

            return age >= 18 ? ValidationResult.Success : new ValidationResult("Bạn phải đủ 18 tuổi.");
        }
        public string BarId { get; set; } //BarId

        public string RoleId { get => "550e8400-e29b-41d4-a716-446655440201"; } //RoleId Staff
        public int Status { get; set; }
    }
}
