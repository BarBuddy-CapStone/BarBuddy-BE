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
        [Required(ErrorMessage = "BarName cannot be empty")]
        [StringLength(50, MinimumLength = 7, ErrorMessage = "BarName must be between 7 and 50 characters")]
        public string BarName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Phone number must contain only digits.")]
        public string PhoneNumber { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public List<string>? Images { get; set; }
        public List<string>? imgsAsString { get; set; }
        [Range(0,100)]
        [Required(ErrorMessage = "More than or equal 0 and less than or equal 100")]
        public double Discount { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        [Range(0,24,ErrorMessage ="Thời gian của một slot chỉ từ 0 đến 24 giờ")]
        public double TimeSlot { get; set; }
    }
}
