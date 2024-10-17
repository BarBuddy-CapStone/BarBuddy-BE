using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class NotificationDetail
    {
        [Key]
        public Guid NotificationDetailId { get; set; } = Guid.NewGuid();
        public Guid AccountId { get; set; }
        public Guid NotificationId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        [ForeignKey("NotificationId")]
        public virtual Notification Notification { get; set; }
    }
}
