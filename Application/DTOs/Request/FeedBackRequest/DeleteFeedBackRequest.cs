using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Request.FeedBackRequest
{
    public class DeleteFeedBackRequest
    {
        [Required(ErrorMessage = "Status không được để trống !")]
        public bool IsDeleted { get; set; }
    }
}
