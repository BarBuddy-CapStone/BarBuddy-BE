using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class ObjectQueryCustom
    {
        public string? Search { get; set; } = null;
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
