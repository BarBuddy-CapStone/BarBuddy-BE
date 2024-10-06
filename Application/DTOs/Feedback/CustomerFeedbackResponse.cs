using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Feedback
{
    public class CustomerFeedbackResponse
    {
        public Guid FeedbackId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAvatar { get; set; }
        public string BarName { get; set; }
        public string BarAddress { get; set; }
        public string BarImage { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
