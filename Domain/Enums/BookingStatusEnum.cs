using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum BookingStatusEnum
    {
        [Description("Pending")]
        Pending = 0,
        [Description("Success")]
        Success = 1,
        [Description("Failed")]
        Failed = 2,
        [Description("IsCheckIn")]
        IsCheckIn = 3,
        [Description("IsCheckOut")]
        IsCheckOut = 4,
    }
}
