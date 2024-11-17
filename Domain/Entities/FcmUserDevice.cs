using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class FcmUserDevice
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid AccountId { get; set; }

        [Required]
        [StringLength(500)]
        public string DeviceToken { get; set; }

        public string? Platform { get; set; } // iOS/Android

        [Required]
        public DateTimeOffset LastLoginAt { get; set; } = DateTimeOffset.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
} 