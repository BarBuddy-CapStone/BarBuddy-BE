using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Constants
{
    public class PrefixKeyConstant
    {
        #region ItemId
        public static readonly string ACCOUNT = "ACC_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string BAR = "BR_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string BOOKING = "BK_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string BOOKING_DRINK = "BD_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string BOOKING_TABLE = "BT_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string DRINK = "D_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string DRINK_CATEGORY = "DC_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string DRINK_EMOTIONAL_CATEGORY = "DEC_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string EMOTIONAL_DRINK_CATEGORY = "A_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string FEEDBACK = "FB_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string PAYMENT_HISTORY = "PH_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string ROLE = "R_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string TABLE = "T_" + Guid.NewGuid().ToString("N").ToUpper();
        public static readonly string TABLE_TYPE = "TT_" + Guid.NewGuid().ToString("N").ToUpper();
        #endregion

        #region Status Bool
        public static readonly bool TRUE = true;
        public static readonly bool FALSE = false;
        #endregion

        #region Role Name
        public static string ADMIN = "ADMIN";
        public static string STAFF = "STAFF";
        public static string CUSTOMER = "CUSTOMER";
        #endregion
    }
}
