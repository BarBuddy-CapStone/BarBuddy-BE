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

        public async Task CreateEventVoucher(Guid eventTimeId, EventVoucherRequest request)
        {
            try
            {
                var isExist = _unitOfWork.EventVoucherRepository
                                            .Get(filter: x => x.EventVoucherName.Equals(request.EventVoucherName) 
                                                        || x.VoucherCode.Equals(request.VoucherCode)
                                                        && x.Status == PrefixKeyConstant.TRUE).FirstOrDefault();
                if (isExist != null)
                {
                    throw new CustomException.InvalidDataException("Voucher Name đã tồn tại, vui lòng thử lại !");
                }

                var response = _mapper.Map<EventVoucher>(request);
                response.Status = PrefixKeyConstant.TRUE;
                response.TimeEventId = eventTimeId;

                await _unitOfWork.EventVoucherRepository.InsertAsync(response);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }
    }
}
