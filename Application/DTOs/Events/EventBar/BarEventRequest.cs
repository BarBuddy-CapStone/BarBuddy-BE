using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events.EventBar
{
    public class BarEventRequest
    {
        public Guid BarId { get; set; }
        public Guid EventId { get; set; }
    }
}
