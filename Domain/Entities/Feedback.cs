using Domain.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("Feedback")]
    public class Feedback
    {
        [Key]
        public Guid FeedbackId { get; set; } = Guid.NewGuid();
        public Guid AccountId {  get; set; }
        public Guid BookingId { get; set; }
        public Guid BarId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }

        [ForeignKey("BarId")]
        public virtual Bar Bar { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        public Feedback()
        {
            CreatedTime = LastUpdatedTime = CoreHelper.SystemTimeNow;
        }
    }
}
