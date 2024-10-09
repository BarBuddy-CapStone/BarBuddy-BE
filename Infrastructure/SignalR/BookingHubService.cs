using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.SignalR
{
    public class BookingHubService : IBookingHubService
    {
        private readonly IHubContext<BookingHub> _hubContext;

        public BookingHubService(IHubContext<BookingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task HoldTable(Guid tableId)
        {
            await _hubContext.Clients.All.SendAsync("TableHoId", tableId);
        }

        public async Task BookedTable(Guid tableId)
        {
            await _hubContext.Clients.All.SendAsync("BookedTable", tableId);
        }

        public async Task ReleaseTable(Guid tableId)
        {
            await _hubContext.Clients.All.SendAsync("TableReleased", tableId);
        }
    }
}
