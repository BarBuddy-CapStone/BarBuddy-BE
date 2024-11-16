using Application.Common;
using Application.DTOs.Events.EventVoucher;
using Application.IService;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class EventVoucherService : IEventVoucherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventVoucherService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateEventVoucher(Guid eventId, EventVoucherRequest request)
        {
            try
            {
                var isExist = _unitOfWork.EventVoucherRepository
                                            .Get(filter: x => x.EventVoucherName.Equals(request.EventVoucherName)
                                                        || x.VoucherCode.Equals(request.VoucherCode)
                                                        && x.Status == PrefixKeyConstant.TRUE).FirstOrDefault();
                if (isExist != null)
                {
                    throw new CustomException.InvalidDataException("Tên voucher hoặc mã voucher đã tồn tại, vui lòng thử lại !");
                }

                var response = _mapper.Map<EventVoucher>(request);
                response.Status = PrefixKeyConstant.TRUE;
                response.EventId = eventId;

                await _unitOfWork.EventVoucherRepository.InsertAsync(response);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }
        public async Task UpdateEventVoucher(Guid eventId, UpdateEventVoucherRequest request)
        {
            try
            {
                var getOneVoucher = await _unitOfWork.EventVoucherRepository.GetByIdAsync(request.EventVoucherId);

                if (getOneVoucher == null)
                {
                    throw new CustomException.DataNotFoundException("Voucher không tồn tại !");
                }

                var mapper = _mapper.Map(request, getOneVoucher);
                await _unitOfWork.EventVoucherRepository.UpdateRangeAsync(mapper);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();

            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
        public async Task DeleteEventVoucher(Guid eventId, Guid eventVoucherId)
        {
            try
            {
                var isExistVoucher = _unitOfWork.EventVoucherRepository
                                                    .Get(filter: x => x.Event.EventId.Equals(eventId))
                                                    .FirstOrDefault();

                if (isExistVoucher == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy voucher !");
                }

                await _unitOfWork.EventVoucherRepository.DeleteAsync(eventVoucherId);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
            }

            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
        public async Task<EventVoucher> GetVoucherBasedEventId(Guid eventId)
        {
            try
            {
                var getVoucher = (await _unitOfWork.EventVoucherRepository
                                                    .GetAsync(filter: x => x.EventId.Equals(eventId)))
                                                    .FirstOrDefault();
                if(getVoucher == null)
                {
                    return null;
                }
                return getVoucher;
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
        public async Task<EventVoucherResponse> GetVoucherByCode(VoucherQueryRequest request)
        {
            try
            {
                var isExistVoucher = _unitOfWork.EventVoucherRepository
                                                .Get(filter: x =>
                                                    x.VoucherCode.Equals(request.voucherCode) &&
                                                    (x.Quantity > 0 || x.Quantity == null) &&
                                                    x.Status == PrefixKeyConstant.TRUE &&
                                                    x.Event.Bar.BarId.Equals(request.barId) &&
                                                    x.Event.TimeEvent.Any(t =>
                                                        (t.DayOfWeek != null && t.DayOfWeek == (int)request.bookingDate.DayOfWeek) ||
                                                        (t.Date != null && t.Date.Value.Date == request.bookingDate.Date)
                                                    ),
                                                    includeProperties: "Event.Bar.BarTimes,Event.TimeEvent")
                                                .FirstOrDefault();

                if (isExistVoucher == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy voucher");
                }
                
                Utils.ValidateTimeWithinEvent(request.bookingDate, request.bookingTime, isExistVoucher.Event.TimeEvent.ToList());

                var response = _mapper.Map<EventVoucherResponse>(isExistVoucher);
                return response;   
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (CustomException.InvalidDataException ex)
            {
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task UpdateStatusVoucher(Guid eventVoucherId)
        {
            try
            {
                var getOneVoucher = await _unitOfWork.EventVoucherRepository.GetByIdAsync(eventVoucherId);
                if (getOneVoucher == null) { 
                    throw new CustomException.DataNotFoundException("Không tìm thấy voucher !");
                }

                await _unitOfWork.EventVoucherRepository.UpdateRangeAsync(getOneVoucher);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task UpdateStatusNStsVoucher(Guid eventVoucherId, int quantityVoucher, bool sts)
        {
            try
            {
                var getOneVoucher = await _unitOfWork.EventVoucherRepository.GetByIdAsync(eventVoucherId);
                if (getOneVoucher == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy voucher !");
                }

                getOneVoucher.Status = sts;
                getOneVoucher.Quantity = quantityVoucher;

                await _unitOfWork.EventVoucherRepository.UpdateRangeAsync(getOneVoucher);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
    }
}
