using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.FeedBackRequest
{
    public class CreateFeedBackRequest
    {
        public Guid BookingId { get; set; }
        //public Guid BarId { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 !")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá không thể trống !")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Nội dung đánh giá phải từ 10 đến 500 kí tự !")]
        public string Comment { get; set; }
    }
}
