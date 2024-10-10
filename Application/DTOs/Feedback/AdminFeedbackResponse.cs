using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Feedback
{
    public class AdminFeedbackResponse
    {
        public Guid FeedbackId { get; set; }
        public string CustomerName { get; set; }
        public string BarName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
    }
}
