using Application.DTOs.BarTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class CreateBarRequest : BarBaseRequest
    {
        [Required]
        public List<BarTimeRequest> BarTimeRequest { get; set; }
    }
}
