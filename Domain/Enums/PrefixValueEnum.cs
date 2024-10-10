using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum PrefixValueEnum
    {
        [Description("Active")]
        Active = 1,
        [Description("Inactive")]
        Inactive = 0,

        [Description("Peding")]
        Peding = 0,
        [Description("Cancelled")]
        Cancelled = 1,
        [Description("Serving")]
        Serving = 2,
        [Description("Completed")]
        Completed = 3


    }
}
