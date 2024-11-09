using Application.DTOs.Events.EventTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IEventTimeService
    {
        Task CreateEventTime(Guid eventId, EventTimeRequest request);
        Task UpdateEventTime(Guid eventId, bool IsEveryWeek, List<UpdateEventTimeRequest> request);
    }
}
