using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("BarTime")]
    public class BarTime
    {
        [Key]
        public Guid BarTimeId { get; set; } = new Guid();
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        [ForeignKey("BarId")]
        public Guid BarId { get; set; }
        public virtual Bar Bar { get; set; }
    }
}
