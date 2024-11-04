using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class OnlyBarIdNameResponse
    {
        public Guid BarId { get; set; }
        public string BarName { get; set; }
    }
}
