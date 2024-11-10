using Application.DTOs.Events.EventVoucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IEventVoucherService
    {
        Task CreateEventVoucher(Guid eventId, EventVoucherRequest request);
        Task DeleteEventVoucher(Guid eventTimeId, List<Guid> eventVoucherId);
        Task UpdateEventVoucher(Guid eventTimeId, List<UpdateEventVoucherRequest> request);
    }
}
