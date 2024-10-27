using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Table
{
    public class TableResponse
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public Guid TableTypeId { get; set; }
        public string TableTypeName { get; set; }
        public int MinimumGuest { get; set; }
        public int MaximumGuest { get; set; }
        public double MinimumPrice { get; set; }
        public int Status { get; set; }
    }
}
