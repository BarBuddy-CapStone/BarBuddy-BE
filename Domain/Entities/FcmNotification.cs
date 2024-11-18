using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class FcmNotification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        public FcmNotificationType Type { get; set; }

        public string? ImageUrl { get; set; }

        public string? DeepLink { get; set; }

        public Guid? BarId { get; set; }

        [Required]
        public bool IsPublic { get; set; } // True: guest + customer, False: customer only

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("BarId")]
        public virtual Bar? Bar { get; set; }
        public virtual ICollection<FcmNotificationCustomer> NotificationCustomers { get; set; }
    }
} 