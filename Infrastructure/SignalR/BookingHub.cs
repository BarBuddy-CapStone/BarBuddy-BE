using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR
{
    public class BookingHub : Hub
    {
        public async Task HoldTable(Guid tableId)
        {
            await Clients.Others.SendAsync("TableHoId", tableId);
        }

        public async Task BookedTable(Guid tableId)
        {
            await Clients.Others.SendAsync("BookedTable", tableId);
        }

        public async Task ReleaseTable(Guid tableId)
        {
            await Clients.Others.SendAsync("TableReleased", tableId);
        }
    }
}
