using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public class PaymentLink
    {
        public string PaymentId { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
    }
}
