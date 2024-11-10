using Application.DTOs.Events.EventTime;

namespace Application.DTOs.Events
{
    public class EventResponse
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public string Images { get; set; }
        public bool IsHide {  get; set; }
        public List<EventTimeResponse> EventTimeResponses { get; set; }
    }
}
