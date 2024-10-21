using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class TimeEvent
    {
        [Key]
        public Guid TimeEventId { get; set; }
        public Guid EventId { get; set; }
        public DateTimeOffset? Date {  get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? DayOfWeek { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        public virtual ICollection<EventVoucher>? EventVouchers { get; set; }
    }
}
