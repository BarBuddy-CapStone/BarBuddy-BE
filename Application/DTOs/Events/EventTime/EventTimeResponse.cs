using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events.EventTime
{
    public class EventTimeResponse
    {
        public DateTimeOffset? Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? DayOfWeek { get; set; }
    }
}
