using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Application.DTOs.Fcm
{
    public class CreateNotificationRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        public FcmNotificationType Type { get; set; }

        public string? ImageUrl { get; set; }

        public string? MobileDeepLink { get; set; }
        
        public string? WebDeepLink { get; set; }

        public Guid? BarId { get; set; }

        [Required]
        public bool IsPublic { get; set; }
        
        public List<Guid>? SpecificAccountIds { get; set; }
    }
} 