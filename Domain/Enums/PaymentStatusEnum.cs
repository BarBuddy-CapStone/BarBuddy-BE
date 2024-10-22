using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum PaymentStatusEnum
    {
        [Description("Pending")]
        Pending = 0,
        [Description("Success")]
        Success = 2,
        [Description("Failed")]
        Failed = 1,
    }
}
