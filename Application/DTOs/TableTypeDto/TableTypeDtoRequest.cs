using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.TableTypeDto
{
    public class TableTypeDtoRequest
    {
        [Required]
        public string? TypeName { get; set; }
        public string? Description { get; set; }
        [Required]
        [Range(1, 99, ErrorMessage = "Chỉ trong khoảng từ 1 - 99")]
        public int MinimumGuest { get; set; }
        [Required]
        [Range(1, 99, ErrorMessage = "Chỉ trong khoảng từ 1 - 99")]
        public int MaximumGuest { get; set; }
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Lớn hơn 0")]
        public double MinimumPrice { get; set; }
    }
}
