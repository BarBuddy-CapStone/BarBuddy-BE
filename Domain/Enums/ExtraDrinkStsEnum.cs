using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum ExtraDrinkStsEnum
    {
        [Description("Pending")]
        Pending = 0,
        [Description("Preparing")]
        Preparing = 1,
        [Description("Delivered")]
        Delivered = 2,

    }
}
