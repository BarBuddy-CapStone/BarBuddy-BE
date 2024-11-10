using Application.Common;
using Application.DTOs.Event;
using Application.DTOs.Events;
using Application.DTOs.Events.EventTime;
using Application.DTOs.Events.EventVoucher;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.Common;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace Application.Service
{
    public class EventService : IEventService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventTimeService _eventTimeService;
        private readonly IFirebase _fireBase;
        private readonly ILogger<EventService> _logger;
        private readonly IEventVoucherService _eventVoucherService;

        public EventService(IMapper mapper, IUnitOfWork unitOfWork, IFirebase fireBase,
                            IEventTimeService eventTimeService, ILogger<EventService> logger, IEventVoucherService eventVoucherService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _eventTimeService = eventTimeService;
            _fireBase = fireBase;
            _logger = logger;
            _eventVoucherService = eventVoucherService;
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

                if (request.EventTimeRequest.Count < 1)
                {
                    throw new CustomException.InvalidDataException("Không thể tạo event khi không có thời gian diễn ra sự kiện !");
                }

                images = Utils.ConvertBase64ListToFiles(request.Images);
                var imgToUpload = Utils.CheckValidateImageFile(images);

                mapper.Images = "";
                mapper.IsDeleted = PrefixKeyConstant.FALSE;
                mapper.IsHide = PrefixKeyConstant.FALSE;
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

                if (request.EventVoucherRequest != null)
                {
                    await _eventVoucherService.CreateEventVoucher(mapper.EventId, request.EventVoucherRequest);
                }

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

        public async Task DeleteEvent(Guid eventId)
        {
            try
            {
                var isExistEvent = _unitOfWork.EventRepository
                                                .Get(filter: x => x.EventId.Equals(eventId) &&
                                                x.IsDeleted == PrefixKeyConstant.FALSE).FirstOrDefault();

                if (isExistEvent == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy event !");
                }

                isExistEvent.IsDeleted = PrefixKeyConstant.TRUE;

                await _unitOfWork.EventRepository.UpdateRangeAsync(isExistEvent);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();

            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task<List<EventResponse>> GetAllEvent(EventQuery query)
        {
            Expression<Func<Event, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                filter = events => events.EventId.ToString().Equals(query.Search);

            }
            var getAll = (await _unitOfWork.EventRepository
                                            .GetAsync(filter: filter,
                                                        pageIndex: query.PageIndex,
                                                        pageSize: query.PageSize,
                                                 includeProperties: "Bar,TimeEvent,EventVoucher"))
                                                 .Where(x => x.IsDeleted == PrefixKeyConstant.FALSE);
            if (getAll.IsNullOrEmpty())
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy dữ liệu");
            }

            var response = _mapper.Map<List<EventResponse>>(getAll);
            foreach (var item in response)
            {
                var events = getAll.First(x => x.EventId.Equals(item.EventId));
                item.EventTimeResponses = _mapper.Map<List<EventTimeResponse>>(events.TimeEvent);
                item.EventVoucherResponse = _mapper.Map<EventVoucherResponse>(events.EventVoucher);
            }
            return response;
        }

        public async Task<List<EventResponse>> GetEventsByBarId(ObjectQuery query, Guid? barId)
        {
            if (!barId.HasValue) throw new CustomException.InvalidDataException(nameof(barId));

            var getAll = (await _unitOfWork.EventRepository.GetAsync(filter: e => e.BarId.Equals(barId),
                    includeProperties: "Bar,TimeEvent,EventVoucher",
                    pageIndex: query.PageIndex,
                    pageSize: query.PageSize
                    )).Where(x => x.IsDeleted == PrefixKeyConstant.FALSE);

            if (!getAll.Any())
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy dữ liệu");
            }

            var response = _mapper.Map<List<EventResponse>>(getAll);
            foreach (var item in response)
            {
                var events = getAll.First(x => x.EventId.Equals(item.EventId));
                item.EventTimeResponses = _mapper.Map<List<EventTimeResponse>>(events.TimeEvent);
                item.EventVoucherResponse = _mapper.Map<EventVoucherResponse>(events.EventVoucher);
            }
            return response;

        }

        public async Task<EventResponse> GetOneEvent(Guid eventId)
        {
            try
            {
                var getEventById = _unitOfWork.EventRepository
                                            .Get(filter: x => x.EventId.Equals(eventId) &&
                                                        x.IsDeleted == PrefixKeyConstant.FALSE
                                            , includeProperties: "Bar,TimeEvent,EventVoucher");
                var getOne = getEventById.FirstOrDefault()
                            ?? throw new CustomException.DataNotFoundException("Không tìm thấy sự kiện bạn đang tìm!");


                var response = _mapper.Map<EventResponse>(getOne);
                response.EventTimeResponses = _mapper.Map<List<EventTimeResponse>>(getOne.TimeEvent);
                response.EventVoucherResponse = _mapper.Map<EventVoucherResponse>(getOne.EventVoucher);
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }

        public async Task UpdateEvent(Guid eventId, UpdateEventRequest request)
        {
            try
            {
                List<string> imgStr = new List<string>();
                List<IFormFile> images = new List<IFormFile>();
                List<IFormFile> imgToUpload = new List<IFormFile>();
                string imgsAsString = string.Empty;
                string oldImgsUploaded = string.Empty;

                if (request.Images.IsNullOrEmpty() && request.OldImages.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Không thể thiếu hình ảnh !");
                }

                if (request.UpdateEventTimeRequests.Count < 1)
                {
                    throw new CustomException.InvalidDataException("Không thể chỉnhh sửa event khi không có thời gian diễn ra sự kiện !");
                }

                if (!request.Images.IsNullOrEmpty())
                {
                    images = Utils.ConvertBase64ListToFiles(request.Images);
                    imgToUpload = Utils.CheckValidateImageFile(images);
                }
                _unitOfWork.BeginTransaction();
                var isExistEvent = _unitOfWork.EventRepository
                                                .Get(filter: e => e.EventId.Equals(eventId) &&
                                                                e.IsDeleted == PrefixKeyConstant.FALSE,
                                                                includeProperties: "TimeEvent.EventVouchers")
                                                .FirstOrDefault();
                if (isExistEvent == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy Event !");
                }

                var mapper = _mapper.Map(request, isExistEvent);
                mapper.Images = "";
                mapper.IsDeleted = PrefixKeyConstant.FALSE;
                mapper.IsHide = PrefixKeyConstant.FALSE;
                await _unitOfWork.EventRepository.UpdateRangeAsync(mapper);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();


                foreach (var img in imgToUpload)
                {
                    var uploadImg = await _fireBase.UploadImageAsync(img);
                    imgStr.Add(uploadImg);
                }

                imgsAsString = string.Join(", ", imgStr);
                oldImgsUploaded = string.Join(", ", request.OldImages);
                mapper.Images = string.IsNullOrEmpty(imgsAsString) ? oldImgsUploaded : $"{imgsAsString},{oldImgsUploaded}";

                await _unitOfWork.EventRepository.UpdateRangeAsync(mapper);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();

                await _eventTimeService.UpdateEventTime(mapper.EventId, mapper.IsEveryWeek, request.UpdateEventTimeRequests);

                await _unitOfWork.SaveAsync();
                _unitOfWork.CommitTransaction();
            }
            catch (Exception ex)
            {
                await _unitOfWork.DisposeAsync();
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
    }
}
