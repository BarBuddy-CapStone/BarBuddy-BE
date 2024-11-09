using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events.EventVoucher
{
    public class UpdateEventVoucherRequest : EventVoucherRequest
    {
        [Required]
        public Guid EventVoucherId { get; set; }
    }
}
