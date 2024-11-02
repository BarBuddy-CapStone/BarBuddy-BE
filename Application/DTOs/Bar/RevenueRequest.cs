using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class RevenueRequest
    {
        [Required]
        public Guid BarId { get; set; }
        [RegularExpression("^(day|month|year)$", ErrorMessage = "Type must be 'day', 'month', or 'year'.")]
        public DateTime? DateTime { get; set; }
        [RegularExpression("^(day|month|year)$", ErrorMessage = "Type must be 'day', 'month', or 'year'.")]
        public string? Type { get; set; }
    }
}
