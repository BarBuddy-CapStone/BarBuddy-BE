using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBookingHubService
    {
        Task HoldTable(Guid tableId);
        Task BookedTable(Guid tableId);
        Task ReleaseTable(Guid tableId);
    }
}
