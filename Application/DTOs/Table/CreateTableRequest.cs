using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Table
{
    public class CreateTableRequest
    {
        [Required(ErrorMessage = "Không thể thiếu loại bàn !")]
        public Guid TableTypeId { get; set; }
        [Required(ErrorMessage = "Tên bàn không thể trống !")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Tên bàn phải từ 6 đến 20 kí tự!")]
        public string TableName { get; set; }
        [Required(ErrorMessage ="Trạng thái bàn không thể trống !")]
        public int Status { get; set; }
    }
}
