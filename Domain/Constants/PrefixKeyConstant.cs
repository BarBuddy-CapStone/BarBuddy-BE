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

        #region Notification
        public static string BOOKING_SUCCESS = "Bạn đã đặt bàn thành công!";
        public static string BOOKING_PENDING_NOTI = "Chúng tôi xin nhắc nhở bạn rằng thời gian check-in của bạn tại quán {0} vào ngày {1} sẽ diễn ra trong khoảng thời gian 1 giờ tới.";
        public static string BOOKING_REMIND_NOTI = "Đã đến giờ check-in bàn tại quán {0} vào ngày {1} vào lúc {2}. Vui lòng đến trước 1 tiếng kể từ lúc thời gian check-in bàn! Chúc bạn vui vẻ khi tận hưởng tại quán.";
        public static string BOOKING_DRINKS_COMPLETED_NOTI = "Bạn đã hoàn thành đơn đặt bàn với đồ uống tại quán {0}. Cảm ơn bạn đã trải nghiệm tại quán.";
        public static string BOOKING_CANCEL_NOTI = "Đơn đặt bàn vào lúc {0} ngày {1} của bạn đã bị hủy do vượt quá giờ quy định tại quán {2}.";
        #endregion
    }
}
