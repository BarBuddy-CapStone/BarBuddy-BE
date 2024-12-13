using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Authen
{
    public class ResetPasswordRequest
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

        [Required(ErrorMessage = "UniqueCode không thể trống !")]
        public string UniqueCode { get; set; }
    }
}
