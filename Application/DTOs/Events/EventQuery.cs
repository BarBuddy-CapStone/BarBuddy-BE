using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events
{
    public class EventQuery : ObjectQuery
    {
        public Guid? BarId { get; set; }
    }
}
