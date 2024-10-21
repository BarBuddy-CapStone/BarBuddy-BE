using Application.DTOs.Events.EventBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IEventBarService
    {
        Task CreateEventBar(BarEventRequest request);
    }
}
