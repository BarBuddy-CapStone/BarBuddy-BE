using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class ObjectQueryHaveDateTime : ObjectQuery
    {
        public DateTime? DateTimePicker { get; set; } = DateTime.Now;
    }
}
