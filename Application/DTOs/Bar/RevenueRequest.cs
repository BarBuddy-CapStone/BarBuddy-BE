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
        public string? BarId { get; set; }
        public DateTime? DateTime { get; set; }
        [RegularExpression("^(day|month|year)$", ErrorMessage = "Type must be 'day', 'month', or 'year'.")]
        public string? Type { get; set; }
    }
}
