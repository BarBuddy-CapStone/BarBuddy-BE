using Application.DTOs.Events.EventTime;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Event
{
    public class EventRequest
    {
        [Required]
        public List<Guid> BarId { get; set; }
        [Required]
        public string EventName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public List<string> Images { get; set; }
        [Required]
        public bool IsEveryWeek { get; set; }
        [Required]
        public List<EventTimeRequest> EventTimeRequest { get; set; }
    }
}
