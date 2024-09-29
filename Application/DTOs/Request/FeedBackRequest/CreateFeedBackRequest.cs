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
        public Guid BarId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Comment cannot be empty")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 500 characters.")]
        public string Comment { get; set; }
    }
}
