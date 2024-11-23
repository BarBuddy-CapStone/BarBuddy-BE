using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class PagingBarResponse : PagingResponse
    {
        public List<BarResponse> BarResponses { get; set; }
    }
}
