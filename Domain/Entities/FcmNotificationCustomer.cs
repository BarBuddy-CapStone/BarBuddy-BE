using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class FcmNotificationCustomer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid NotificationId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        public bool IsRead { get; set; }

        public DateTimeOffset? ReadAt { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        [ForeignKey("NotificationId")]
        public virtual FcmNotification Notification { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Account Customer { get; set; }
    }
} 