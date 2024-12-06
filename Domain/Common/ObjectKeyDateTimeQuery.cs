using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class ObjectKeyDateTimeQuery
    {
        public Guid? BarId {  get; set; } 
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
