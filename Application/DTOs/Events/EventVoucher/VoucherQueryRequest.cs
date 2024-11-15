using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events.EventVoucher
{
    public class VoucherQueryRequest
    {
        [Required]
        public Guid barId { get; set; }
        [Required] 
        public DateTimeOffset bookingDate { get; set; }
        [Required]
        public TimeSpan bookingTime { get; set; } 
        [Required]
        public string voucherCode { get; set; }
    }
}
