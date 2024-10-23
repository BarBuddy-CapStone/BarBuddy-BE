using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Response.FeedBack
{
    public class FeedBackResponse
    {
        //public Guid FeedbackId { get; set; } 
        //public Guid AccountId { get; set; }
        public Guid BookingId { get; set; }
        public string? ImageAccount { get; set; }
        public string? AccountName { get; set; }
        public Guid BarId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
    }
}
