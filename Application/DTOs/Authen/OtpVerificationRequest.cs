using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Authen
{
    public class OtpVerificationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "TableName cannot be empty")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Otp must be 6 characters")]
        public string Otp { get; set; }
    }
}
