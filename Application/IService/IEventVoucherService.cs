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
        Task CreateEventVoucher(Guid eventTimeId, EventVoucherRequest request);
    }
}
