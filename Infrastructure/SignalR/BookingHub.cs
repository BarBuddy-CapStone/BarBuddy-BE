using Application.DTOs.BookingTable;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR
{
    public class BookingHub : Hub
    {
        public async Task HoldTable(BookingHubResponse response)
        {
            await Clients.Others.SendAsync("TableHoId", response);
        }

        public async Task BookedTable(BookingHubResponse response)
        {
            await Clients.Others.SendAsync("BookedTable", response);
        }

        public async Task ReleaseTable(BookingHubResponse response)
        {
            await Clients.Others.SendAsync("TableReleased", response);
        }
    }
}
