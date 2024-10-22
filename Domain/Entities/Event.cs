using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Event
    {
        [Key]
        public Guid EventId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }
        public bool IsEveryWeek { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<TimeEvent> TimeEvent { get; set; }
        public virtual ICollection<BarEvent> BarEvent { get; set; }
    }
}
