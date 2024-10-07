using Application.DTOs.BookingTable;
using Application.DTOs.TableType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IBookingTableService
    {
        Task<FilterTableTypeReponse> FilterTableTypeReponse(FilterTableDateTimeRequest request);
    }
}
