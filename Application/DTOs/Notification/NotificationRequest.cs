﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Notification
{
    public class NotificationRequest
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
    }
}