using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Table
{
    public class FilterTableResponse
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public int Status { get; set; }
        public int MinimumGuest { get; set; }
        public int MaximumGuest { get; set; }
    }
}
