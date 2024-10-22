using Application.DTOs.Events.EventBar;
using Application.DTOs.Events.EventTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events
{
    public class EventResponse
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public string Images { get; set; }
        public List<BarEventResponse> BarEventResponses { get; set; }
        public List<EventTimeResponse> EventTimeResponses { get; set; }
    }
}
