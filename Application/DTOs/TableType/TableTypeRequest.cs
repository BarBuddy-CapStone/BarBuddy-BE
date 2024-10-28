using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.TableType
{
    public class TableTypeRequest
    {
        [Required]
        public Guid BarId { get; set; }
        [Required(ErrorMessage ="Thể loại bàn không thể trống !")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Thể loại bàn phải từ 7 đến 100 kí tự !")]
        public string? TypeName { get; set; }
        [Required(ErrorMessage = "Mô tả không thể trống !")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Mô tả phải từ 7 đến 100 kí tự !")]
        public string? Description { get; set; }
        [Required]
        [Range(1, 99, ErrorMessage = "Chỉ trong khoảng từ 1 - 99")]
        public int MinimumGuest { get; set; }
        [Required]
        [Range(1, 99, ErrorMessage = "Chỉ trong khoảng từ 1 - 99")]
        public int MaximumGuest { get; set; }
        [Required]
        [Range(0, 100000000, ErrorMessage = "Lớn hơn 0 và bé hơn 100000000")]
        public double MinimumPrice { get; set; }
    }
}
