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
        public Guid BarId { get; set; }
        public Guid TableTypeId { get; set; }
        [Required(ErrorMessage = "TableName cannot be empty")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "TableName must be between 6 and 20 characters")]
        public string TableName { get; set; }
        public int Status { get; set; }
    }
}
