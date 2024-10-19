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
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Notification()
        {
            CreatedAt = CoreHelper.SystemTimeNow;
        }
        public virtual ICollection<NotificationDetail> NotificationDetails { get; set; }
    }
}
