using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs.Account
{
    public class PaginationList<T> where T : class
    {
        public IEnumerable<T> items;
        public int count;
        //public int pageSize;
        //public int pageIndex;
    }
}
