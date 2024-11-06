using Application.DTOs.BookingTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBookingHubService
    {
        Task HoldTable(BookingHubResponse response);
        Task BookedTable(BookingHubResponse response);
        Task ReleaseTable(BookingHubResponse response);
        Task ReleaseListTablee(Guid barId);
    }
}
