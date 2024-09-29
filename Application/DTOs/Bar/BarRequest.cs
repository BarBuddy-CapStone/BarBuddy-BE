using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class BarRequest
    {
        [Required]
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
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
        [Required]
        public List<IFormFile> Images { get; set; }
        [Range(0,100)]
        [Required(ErrorMessage = "More than or equal 0 and less than or equal 100")]
        public double Discount { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}
