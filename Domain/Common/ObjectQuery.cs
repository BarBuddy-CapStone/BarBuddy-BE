using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class ObjectQuery
    {
        public string? Search { get; set; } = null;
        public int? PageIndex { get; set; } = 1;
        public int? PageSize { get; set; } = 6;
    }
}
