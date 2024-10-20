using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class BarEvent
    {
        [Key]
        public Guid BarEventId { get; set; }
        public Guid BarId { get; set; }
        public Guid EventId { get; set; }
        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }
}
