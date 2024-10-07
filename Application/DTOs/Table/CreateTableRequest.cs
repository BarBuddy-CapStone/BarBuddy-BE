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
        public string TableName { get; set; }
        public int Status { get; set; }
    }
}
