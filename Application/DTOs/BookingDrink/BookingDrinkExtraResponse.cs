using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingDrink
{
    public class BookingDrinkExtraResponse : BookingDrinkDetailResponse
    {
        public Guid? CustomerId { get; set; }
        public Guid? StaffId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int Status { get; set; }
    }
}
