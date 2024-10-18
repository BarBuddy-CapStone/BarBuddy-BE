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

        public async Task ReleaseTable(Guid barId)
        {
            await Clients.All.SendAsync("TableReleased", barId);
        }

        public async Task ReleaseListTable(Guid barId)
        {
            await Clients.Group(barId.ToString()).SendAsync("TableListReleased", barId);
        }
    }
}
