using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class FcmUserDevice
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? AccountId { get; set; }

        [Required]
        public string DeviceToken { get; set; }

        [Required]
        public string Platform { get; set; }

        public bool IsGuest { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? LastActiveAt { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }
    }
} 