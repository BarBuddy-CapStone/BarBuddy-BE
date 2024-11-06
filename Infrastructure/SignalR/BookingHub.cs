using Application.DTOs.BookingTable;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR
{
    public class BookingHub : Hub
    {
        public async Task HoldTable(BookingHubResponse response)
        {
            await Clients.All.SendAsync("TableHoId", response);
        }

        public async Task BookedTable(BookingHubResponse response)
        {
            await Clients.All.SendAsync("BookedTable", response);
        }

        public async Task ReleaseTable(BookingHubResponse response)
        {
            await Clients.All.SendAsync("TableReleased", response);
        }

        public async Task ReleaseListTable(BookingHubResponse response)
        {
            await Clients.Group(response.BarId.ToString()).SendAsync("TableListReleased", response);
        }
    }
}
