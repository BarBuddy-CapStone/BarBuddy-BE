using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Events
{
    public class EventQuery : ObjectQuery
    {
        public Guid? BarId { get; set; }
        [Range(0, 1, ErrorMessage = "Giá trị chỉ từ 1 và 2")]
        public int? IsStill {  get; set; }
    }
}
