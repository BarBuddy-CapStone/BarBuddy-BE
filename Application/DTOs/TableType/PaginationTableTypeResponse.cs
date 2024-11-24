using Application.DTOs.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.TableType
{
    public class PaginationTableTypeResponse
    {
        public List<TableTypeResponse> TableTypeResponses { get; set; }
        public int TotalPages { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
    }
}
