using Application.DTOs.Booking;
using Application.DTOs.BookingTable;
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

        public async Task HoldTable(BookingHubResponse response)
        {
            await _hubContext.Clients.All.SendAsync("TableHoId", response);
        }

        public async Task BookedTable(BookingHubResponse response)
        {
            await _hubContext.Clients.All.SendAsync("BookedTable", response);
        }

        public async Task ReleaseTable(Guid barId)
        {
            await _hubContext.Clients.All.SendAsync("TableReleased", barId);
        }

        public async Task ReleaseListTablee(Guid barId)
        {
            await _hubContext.Clients.All.SendAsync("TableListReleased", barId);
        }
    }
}
