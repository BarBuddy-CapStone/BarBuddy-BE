using Application.DTOs.BarTime;
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
    public class BarTimeService : IBarTimeService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public BarTimeService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateBarTimeOfBar(Guid barId, List<BarTimeRequest> request)
        {
            try
            {
                var dayOfWeekList = request.Select(x => x.DayOfWeek).ToList();
                if (dayOfWeekList.Distinct().Count() != dayOfWeekList.Count())
                {
                    throw new CustomException.InvalidDataException("Giá trị DayOfWeek trong danh sách không thể trùng lặp, vui lòng thử lại.");
                }

                var isBarHasTime = _unitOfWork.BarRepository.Get(filter: x => x.BarId.Equals(barId)).FirstOrDefault();

                if (isBarHasTime?.BarTimes != null)
                {
                    throw new CustomException.InvalidDataException("Dữ liệu Không hợp lệ !");
                }

                foreach (var barTime in request)
                {
                    var mapper = _mapper.Map<BarTime>(barTime);
                    mapper.BarId = barId;
                    await _unitOfWork.BarTimeRepository.InsertAsync(mapper);
                }
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task UpdateBarTimeOfBar(Guid barId, List<UpdateBarTimeRequest> request)
        {
            try
            {
                var getBarTimeOfBar = _unitOfWork.BarTimeRepository.Get(filter: x => x.BarId.Equals(barId)).ToList();

                if (getBarTimeOfBar.IsNullOrEmpty())
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy thời gian mở cửa đóng cửa của quán Bar !");
                }


                foreach (var barTimeReq in request)
                {
                    var existingBarTime = getBarTimeOfBar.FirstOrDefault(x => x.BarTimeId == barTimeReq.BarTimeId);
                    if (existingBarTime != null)
                    {
                        existingBarTime.StartTime = barTimeReq.StartTime;
                        existingBarTime.EndTime = barTimeReq.EndTime;
                        existingBarTime.DayOfWeek = barTimeReq.DayOfWeek;
                        await _unitOfWork.BarTimeRepository.UpdateRangeAsync(existingBarTime);
                    }
                    else
                    {
                        var mapper = _mapper.Map<BarTime>(barTimeReq);
                        mapper.BarId = barId;
                        await _unitOfWork.BarTimeRepository.InsertAsync(mapper);
                    }
                }

                var newDayOfWeek = request.Select(x => x.DayOfWeek).ToList();
                var barTimeToDelete = getBarTimeOfBar.Where(x => !newDayOfWeek.Contains(x.DayOfWeek)).ToList();

                foreach(var dayToDel in barTimeToDelete)
                {
                    await _unitOfWork.BarTimeRepository.DeleteAsync(dayToDel.BarTimeId);
                }
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task DeleteBarTimeOfBar(Guid barId, UpdateBarTimeRequest request)
        {
            try
            {
                var getBarTimeOfBar = _unitOfWork.BarTimeRepository
                                                    .Get(filter: x => x.BarId.Equals(barId)
                                                              && x.BarTimeId.Equals(request.BarTimeId))
                                                    .FirstOrDefault();

                if (getBarTimeOfBar == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy thời gian mở cửa đóng cửa của quán Bar");
                }

                await _unitOfWork.BarTimeRepository.DeleteAsync(getBarTimeOfBar.BarTimeId);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
