using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.BarTime
{
    public class UpdateBarTimeRequest : BarTimeRequest
    {
        public Guid? BarTimeId { get; set; }
    }
}
