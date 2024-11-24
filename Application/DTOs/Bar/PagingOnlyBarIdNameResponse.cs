using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Bar
{
    public class PagingOnlyBarIdNameResponse : PagingResponse
    {
        public List<OnlyBarNameIdResponse> OnlyBarIdNameResponses { get; set; }
    }
}
