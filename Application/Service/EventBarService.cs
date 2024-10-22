using Application.DTOs.Events.EventBar;
using Application.IService;
using AutoMapper;
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
    public class EventBarService : IEventBarService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public EventBarService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateEventBar(BarEventRequest request)
        {
            try
            {
                var isExistBar = _unitOfWork.BarRepository.GetByID(request.BarId);

                if(isExistBar == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy quán bar");
                }

                var response = _mapper.Map<BarEvent>(request);

                await _unitOfWork.BarEventRepository.InsertAsync(response);
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
