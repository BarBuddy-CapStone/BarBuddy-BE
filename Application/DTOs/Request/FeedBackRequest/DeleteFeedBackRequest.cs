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
        [Required(ErrorMessage = "Status cannot be empty")]
        public bool IsDeleted { get; set; }
    }
}
