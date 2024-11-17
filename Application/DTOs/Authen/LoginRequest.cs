using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Authen
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email không thể để trống!")]
        [EmailAddress]
        [StringLength(100, ErrorMessage ="Email tối đa 100 kí tự!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password không thể để trống")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password phải từ 6 đến 20 kí tự !")]
        public string Password { get; set; }
    }
}
