using Application.DTOs.Events.EventVoucher;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IEventVoucherService
    {
        Task<EventVoucher> GetVoucherBasedEventId(Guid eventId);
        Task CreateEventVoucher(Guid eventId, EventVoucherRequest request);
        Task DeleteEventVoucher(Guid eventId, Guid VoucherId);
        Task UpdateEventVoucher(Guid eventId, UpdateEventVoucherRequest request);
        Task<EventVoucherResponse> GetVoucherByCode(VoucherQueryRequest request);
        Task UpdateStatusVoucher(Guid eventVoucherId);
        Task UpdateStatusNStsVoucher(Guid eventVoucherId, int quantityVoucher, bool sts);
    }
}
