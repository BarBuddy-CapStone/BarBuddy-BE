using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Notification
{
    public class NotificationRequest
    {
        [Required(ErrorMessage ="Tựa thông báo không thể để trống!")]
        [StringLength(50,MinimumLength =7,ErrorMessage =("Tựa thông báo phải từ 7 đến 50 kí tự !"))]
        public string Title { get; set; }
        [Required(ErrorMessage = "Nội dung thông báo không thể để trống!")]
        [StringLength(100, MinimumLength = 7, ErrorMessage = ("Nội dung thông báo phải từ 7 đến 100 kí tự !"))]
        public string Message { get; set; }
    }
}
