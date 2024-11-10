using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Event
    {
        [Key]
        public Guid EventId { get; set; }
        public Guid BarId { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }
        public bool IsEveryWeek { get; set; }
        public bool IsHide {  get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<TimeEvent> TimeEvent { get; set; }
        public virtual EventVoucher EventVoucher { get; set; }
        [ForeignKey("BarId")]
        public virtual Bar? Bar { get; set; }
    }
}
