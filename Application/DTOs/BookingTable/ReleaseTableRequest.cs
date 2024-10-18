using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingTable
{
    public class ReleaseTableRequest
    {
        [Required]
        public Guid TableId { get; set; }
        [Required]
        public TimeSpan Time { get; set; }
    }
}
