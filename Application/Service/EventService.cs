using Application.Common;
using Application.DTOs.Event;
using Application.DTOs.Events.EventBar;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class EventService : IEventService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventTimeService _eventTimeService;
        private readonly IEventBarService _eventBarService;
        private readonly IFirebase _fireBase;

        public EventService(IMapper mapper, IUnitOfWork unitOfWork, IFirebase fireBase,
                            IEventTimeService eventTimeService, IEventBarService eventBarService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _eventTimeService = eventTimeService;
            _eventBarService = eventBarService;
            _fireBase = fireBase;
        }

        public async Task CreateEvent(EventRequest request)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                List<string> imgStr = new List<string>();
                List<IFormFile> images = new List<IFormFile>();
                var mapper = _mapper.Map<Event>(request);

                if (request.Images.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Không thể thiếu hình ảnh !");
                }


                images = Utils.ConvertBase64ListToFiles(request.Images);
                var imgToUpload = Utils.CheckValidateImageFile(images);

                mapper.Images = "";
                mapper.IsDeleted = PrefixKeyConstant.FALSE;
                await _unitOfWork.EventRepository.InsertAsync(mapper);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();

                foreach (var img in imgToUpload)
                {
                    var imgUploaded = await _fireBase.UploadImageAsync(img);
                    imgStr.Add(imgUploaded);
                }

                var listImgToStr = string.Join(", ", imgStr);
                mapper.Images = listImgToStr;
                await _unitOfWork.EventRepository.UpdateAsync(mapper);
                foreach (var eventTime in request.EventTimeRequest)
                {
                    if (request.IsEveryWeek == PrefixKeyConstant.FALSE && eventTime.DayOfWeek != null)
                    {
                        throw new CustomException.InvalidDataException("Data không hợp lệ khi sự kiện không diễn ra hàng tuần mà có ngày diễn ra !");
                    }

                    if (request.IsEveryWeek == PrefixKeyConstant.TRUE && eventTime.DayOfWeek == null)
                    {
                        throw new CustomException.InvalidDataException("Data không hợp lệ khi sự kiện diễn ra hàng tuần mà không có ngày diễn ra !");
                    }

                    if (eventTime.DayOfWeek == null && eventTime.Date == null)
                    {
                        throw new CustomException.InvalidDataException("Ngày trong tuần và ngày diễn ra sự kiện, cả 2 không thể không có giá trị cùng một lúc !");
                    }

                    if (eventTime.DayOfWeek != null && eventTime.Date != null)
                    {
                        throw new CustomException.InvalidDataException("Ngày trong tuần và ngày diễn ra sự kiện, cả 2 không thể có giá trị cùng một lúc !");
                    }

                    foreach (var barId in request.BarId)
                    {
                        var getbar = _unitOfWork.BarRepository.GetByID(barId);


                        Utils.ValidateEventTime(eventTime.Date, eventTime.StartTime, eventTime.EndTime, getbar.StartTime, getbar.EndTime);
                        var eventBar = new BarEventRequest
                        {
                            BarId = barId,
                            EventId = mapper.EventId
                        };
                        await _eventBarService.CreateEventBar(eventBar);
                    }
                    await _eventTimeService.CreateEventTime(mapper.EventId, eventTime);
                }
                await _unitOfWork.SaveAsync();
                _unitOfWork.CommitTransaction();
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }
    }
}
