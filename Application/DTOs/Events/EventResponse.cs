using Application.DTOs.Events.EventTime;
using Application.DTOs.Events.EventVoucher;

namespace Application.DTOs.Events
{
    public class EventResponse
    {
        public Guid EventId { get; set; }
        public Guid BarId { get; set; }
        public string BarName { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }
        public string Description { get; set; }
        public bool IsHide {  get; set; }
        public int? IsStill {  get; set; }
        public EventVoucherResponse EventVoucherResponse { get; set; }
        public List<EventTimeResponse> EventTimeResponses { get; set; }
    }
}
