using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Account
{
    public class PagingCustomerAccountResponse : PagingResponse
    {
        public List<CustomerAccountResponse> CustomerAccounts { get; set; }
    }
}
