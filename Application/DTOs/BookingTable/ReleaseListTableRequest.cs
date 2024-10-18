using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BookingTable
{
    public class ReleaseListTableRequest
    {
        [Required]
        public Guid BarId { get; set; }
        [Required]
        public DateTimeOffset Date { get; set; }
        [Required]
        public List<ReleaseTableRequest>? Table { get; set; }
    }
}
