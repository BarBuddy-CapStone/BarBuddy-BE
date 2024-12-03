using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class NotificationAccount
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NotificationId { get; set; }

        [Required]
        public Guid AccountId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTimeOffset? ReadAt { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        [ForeignKey("NotificationId")]
        public virtual FcmNotification Notification { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}
