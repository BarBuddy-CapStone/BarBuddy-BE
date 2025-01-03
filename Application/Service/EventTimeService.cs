﻿using Application.Common;
using Application.DTOs.Events.EventTime;
using Application.IService;
using AutoMapper;
using Domain.Constants;
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
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }

        public async Task UpdateEventTime(Guid eventId, bool IsEveryWeek, List<UpdateEventTimeRequest> request)
        {
            try
            {
                var getBarTime = _unitOfWork.BarTimeRepository.Get(filter: x => x.Bar.Event.Any(x => x.EventId.Equals(eventId))).ToList();

                var getEvTimeBaseEventId = _unitOfWork.TimeEventRepository
                                                        .Get(filter: x => x.EventId.Equals(eventId) &&
                                                                        x.Event.IsDeleted == PrefixKeyConstant.FALSE);

                var newEventTime = request.Where(x => x.TimeEventId == null);

                var updateEventTime = request.Where(x => x.TimeEventId != null)
                                            .Join(getEvTimeBaseEventId, n => n.TimeEventId, o => o.TimeEventId, (n, o) => n)
                                            .ToList();

                var deleteEventTime = getEvTimeBaseEventId.Where(o => request.All(n => n.TimeEventId != o.TimeEventId)).Select(x => x.TimeEventId).ToList();

                foreach (var evTime in request)
                {
                    if (IsEveryWeek == PrefixKeyConstant.FALSE && evTime.DayOfWeek != null)
                    {
                        throw new CustomException.InvalidDataException("Data không hợp lệ khi sự kiện không diễn ra hàng tuần mà có ngày diễn ra !");
                    }

                    if (IsEveryWeek == PrefixKeyConstant.TRUE && evTime.DayOfWeek == null)
                    {
                        throw new CustomException.InvalidDataException("Data không hợp lệ khi sự kiện diễn ra hàng tuần mà không có ngày diễn ra !");
                    }

                    if (evTime.DayOfWeek == null && evTime.Date == null)
                    {
                        throw new CustomException.InvalidDataException("Ngày trong tuần và ngày diễn ra sự kiện, cả 2 không thể không có giá trị cùng một lúc !");
                    }

                    if (evTime.DayOfWeek != null && evTime.Date != null)
                    {
                        throw new CustomException.InvalidDataException("Ngày trong tuần và ngày diễn ra sự kiện, cả 2 không thể có giá trị cùng một lúc !");
                    }

                    Utils.ValidateEventTime(evTime.Date, evTime.StartTime, evTime.EndTime, getBarTime);
                }

                foreach (var eventTime in newEventTime)
                {
                    var mapper = _mapper.Map<EventTimeRequest>(eventTime);
                    await CreateEventTime(eventId, mapper);
                }

                foreach (var eventTime in updateEventTime)
                {
                    var getOne = _unitOfWork.TimeEventRepository.GetByID(eventTime.TimeEventId);
                    
                    if(!getOne.EventId.Equals(eventId))
                    {
                        throw new CustomException.InvalidDataException("Không tìm thấy thời gian diễn ra của sự kiện !");
                    }
                    var mapper = _mapper.Map(eventTime, getOne);

                    await _unitOfWork.TimeEventRepository.UpdateRangeAsync(getOne);
                    await Task.Delay(10);
                    await _unitOfWork.SaveAsync();
                }

                await DeleteEventTime(eventId, deleteEventTime);
            }
            catch (CustomException.InvalidDataException ex)
            {
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task DeleteEventTime(Guid eventId, List<Guid> eventTimeId)
        {
            try
            {


                foreach (var id in eventTimeId)
                {
                    var isExistEvent = _unitOfWork.TimeEventRepository
                                                    .Get(x => x.TimeEventId.Equals(id) &&
                                                            x.Event.EventId.Equals(eventId) &&
                                                            x.Event.IsDeleted == PrefixKeyConstant.FALSE)
                                                    .FirstOrDefault();


                    if (isExistEvent == null)
                    {
                        continue;
                    }

                    //await _eventVoucherService.DeleteEventVoucher(id, isExistEvent?.EventVouchers?.Select(x => x.EventVoucherId).ToList());

                    await _unitOfWork.TimeEventRepository.DeleteAsync(id);
                    await Task.Delay(10);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
    }
}
