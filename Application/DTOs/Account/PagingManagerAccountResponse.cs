using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Account
{
    public class PagingManagerAccountResponse : PagingResponse
    {
        public List<ManagerAccountResponse> ManagerAccountResponses { get; set; }
    }
}
