using Application.DTOs.Events.EventTime;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class EventTimeService : IEventTimeService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventVoucherService _eventVoucherService;
        public EventTimeService(IMapper mapper, IUnitOfWork unitOfWork, IEventVoucherService eventVoucherService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _eventVoucherService = eventVoucherService;
        }

        public async Task CreateEventTime(Guid eventId, EventTimeRequest request)
        {
            try
            {
                var mapper = _mapper.Map<TimeEvent>(request);
                mapper.EventId = eventId;

                await _unitOfWork.TimeEventRepository.InsertAsync(mapper);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();

                foreach (var voucher in request.EventVoucherRequest)
                {
                    await _eventVoucherService.CreateEventVoucher(mapper.TimeEventId, voucher);
                }


            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }
    }
}
