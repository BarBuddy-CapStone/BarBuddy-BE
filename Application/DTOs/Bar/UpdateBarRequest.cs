using Application.DTOs.BarTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class UpdateBarRequest : BarBaseRequest
    {
        [Required]
        public Guid BarId { get; set; }
        [Required]
        public List<UpdateBarTimeRequest> UpdateBarTimeRequests { get; set; }
    }
}
