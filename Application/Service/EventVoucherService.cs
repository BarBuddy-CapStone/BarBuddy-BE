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
                    throw new CustomException.InvalidDataException("Voucher Name hoặc voucher Code  đã tồn tại, vui lòng thử lại !");
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
        public async Task UpdateEventVoucher(Guid eventTimeId, List<UpdateEventVoucherRequest> request)
        {
            try
            {
                foreach (var voucher in request)
                {
                    var getOneVoucher = await _unitOfWork.EventVoucherRepository.GetByIdAsync(voucher.EventVoucherId);

                    if(getOneVoucher == null)
                    {
                        throw new CustomException.DataNotFoundException("Voucher không tồn tại !");
                    }

                    var mapper = _mapper.Map(voucher, getOneVoucher);
                    await _unitOfWork.EventVoucherRepository.UpdateRangeAsync(mapper);
                    await Task.Delay(10);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
        public async Task DeleteEventVoucher(Guid eventId, List<Guid> eventVoucherId)
        {
            try
            {
                foreach (var id in eventVoucherId)
                {
                    var isExistVoucher = _unitOfWork.EventVoucherRepository
                                                        .Get(filter: x => x.Event.EventId.Equals(eventId))
                                                        .FirstOrDefault();

                    if (isExistVoucher != null)
                    {
                        await _unitOfWork.EventVoucherRepository.DeleteAsync(id);
                        await Task.Delay(10);
                        await _unitOfWork.SaveAsync();
                    }
                }
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
    }
}
