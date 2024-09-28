using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.TableTypeDto
{
    public class TableTypeDtoResponse
    {
        public string? TableTypeId { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }
        public int MinimumGuest { get; set; }
        public int MaximumGuest { get; set; }
        public double MinimumPrice { get; set; }
    }
}
