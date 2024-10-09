using Application.DTOs.BookingTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.TableType
{
    public class FilterTableTypeReponse
    {
        public Guid TableTypeId { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public List<FilterBkTableResponse> BookingTables { get; set; }
    }
}
