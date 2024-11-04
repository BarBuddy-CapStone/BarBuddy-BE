using Application.DTOs.Bar;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.IRepository;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Transactions;
using Domain.Constants;
using Microsoft.AspNetCore.Http;
using Application.Common;
using Domain.Enums;
using Domain.Utils;
using Domain.Common;
using System.Linq.Expressions;
using Application.DTOs.BarTime;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Service
{
    public class BarService : IBarService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebase _fireBase;
        private readonly IBarTimeService _barTimeService;
        public BarService(IUnitOfWork unitOfWork, IMapper mapper,
                            IFirebase fireBase, IBarTimeService barTimeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBase = fireBase;
            _barTimeService = barTimeService;
        }

        public async Task CreateBar(CreateBarRequest request)
        {
            string imageUrl = null;
            List<IFormFile> fileToUpload = new List<IFormFile>();
            List<string> listImgs = new List<string>();

            using (TransactionScope transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (request.Images.IsNullOrEmpty())
                    {
                        throw new CustomException.InvalidDataException("Dữ liệu không hợp lí");
                    }

                    var isBarName = _unitOfWork.BarRepository.Get(filter: x => x.BarName.Contains(request.BarName)).FirstOrDefault();

                    if (isBarName != null)
                    {
                        throw new CustomException.InvalidDataException("Tên quán Bar đã tồn tại, vui lòng thử lại !");
                    }

                    var images = Utils.ConvertBase64ListToFiles(request.Images);

                    fileToUpload = Utils.CheckValidateImageFile(images);

                    //Create với img là ""
                    var mapper = _mapper.Map<Bar>(request);
                    mapper.Images = "";
                    await _unitOfWork.BarRepository.InsertAsync(mapper);
                    foreach (var isValidTimeSlot in request.BarTimeRequest)
                    {
                        var isValid = Utils.IValidSlot(request.TimeSlot, isValidTimeSlot.StartTime, isValidTimeSlot.EndTime);
                        if (!isValid)
                        {
                            throw new CustomException.InvalidDataException($" Thời gian đóng mở cửa không đủ cho một slot vào {Utils.GetDayName(isValidTimeSlot.DayOfWeek)}!");
                        }
                    }
                    await _barTimeService.CreateBarTimeOfBar(mapper.BarId, request.BarTimeRequest);
                    await Task.Delay(10);
                    await _unitOfWork.SaveAsync();

                    foreach (var image in fileToUpload)
                    {
                        imageUrl = await _fireBase.UploadImageAsync(image);
                        listImgs.Add(imageUrl);
                    }

                    //Sau khi Upd ở trên thành công => lưu img lên firebase
                    var imagesAsString = string.Join(",", listImgs);
                    mapper.Images = imagesAsString;
                    await _unitOfWork.BarRepository.UpdateAsync(mapper);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    transaction.Complete();
                }
                catch (CustomException.InvalidDataException ex)
                {
                    transaction.Dispose();
                    throw new CustomException.InvalidDataException(ex.Message);
                }
                catch (CustomException.InternalServerErrorException ex)
                {
                    transaction.Dispose();
                    throw new CustomException.InternalServerErrorException(ex.Message);
                }
            }
        }

        public async Task<IEnumerable<BarResponse>> GetAllBar(ObjectQuery query)
        {
            Expression<Func<Bar, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                filter = bar => bar.BarName.Contains(query.Search);
            }

            var getAllBar = await _unitOfWork.BarRepository
                                                .GetAsync(filter: filter,
                                                    pageIndex: query.PageIndex,
                                                    pageSize: query.PageSize,
                                                    includeProperties: "BarTimes");

            if (getAllBar.IsNullOrEmpty() || !getAllBar.Any())
            {
                throw new CustomException.DataNotFoundException("Danh sách đang trống !");
            }
            var response = _mapper.Map<IEnumerable<BarResponse>>(getAllBar);

            foreach (var barTime in response)
            {
                var getOneBar = getAllBar.Where(x => x.BarId.Equals(barTime.BarId)).FirstOrDefault();
                barTime.BarTimeResponses = _mapper.Map<List<BarTimeResponse>>(getOneBar?.BarTimes);
            };

            return response;
        }

        public async Task<IEnumerable<OnlyBarResponse>> GetAllBarWithFeedback(ObjectQuery query)
        {

            Expression<Func<Bar, bool>> filter = null;

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                filter = barName => barName.BarName.Contains(query.Search);
            }

            var bars = await _unitOfWork.BarRepository
                                    .GetAsync(filter: filter,
                                              pageIndex: query.PageIndex,
                                              pageSize: query.PageSize,
                                              includeProperties: "Feedbacks,BarTimes");

            var currentDateTime = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);
            var response = new List<OnlyBarResponse>();



            if (bars.IsNullOrEmpty() || !bars.Any())
            {
                throw new CustomException.DataNotFoundException("Danh sách đang trống !");
            }

            foreach (var bar in bars)
            {
                var tables = await _unitOfWork.TableRepository
                                                .GetAsync(t => t.IsDeleted == false);
                bool isAnyTableAvailable = false;
                foreach (var table in tables)
                {
                    var reservations = await _unitOfWork.BookingTableRepository
                        .GetAsync(filter: bt => bt.TableId == table.TableId &&
                        (bt.Booking.BookingDate + bt.Booking.BookingTime) >= currentDateTime &&
                        (bt.Booking.Status == (int)PrefixValueEnum.Pending || bt.Booking.Status == (int)PrefixValueEnum.Serving),
                        includeProperties: "Booking");

                    if (!reservations.Any())
                    {
                        isAnyTableAvailable = true;
                        break;
                    }
                }
                var mapper = _mapper.Map<OnlyBarResponse>(bar);
                mapper.IsAnyTableAvailable = isAnyTableAvailable;
                mapper.BarTimeResponses = _mapper.Map<List<BarTimeResponse>>(bar.BarTimes);
                response.Add(mapper);
            }
            return response;
        }

        public async Task<BarResponse> GetBarById(Guid barId)
        {
            var getBarById = (await _unitOfWork.BarRepository
                                                    .GetAsync(filter: x => x.BarId.Equals(barId),
                                                              includeProperties: "BarTimes"))
                                                              .FirstOrDefault();

            if (getBarById == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy quán bar !");
            }

            var response = _mapper.Map<BarResponse>(getBarById);
            response.BarTimeResponses = _mapper.Map<List<BarTimeResponse>>(getBarById.BarTimes);
            return response;
        }

        public async Task<BarResponse> GetBarByIdWithFeedback(Guid barId)
        {
            bool isAnyTableAvailable = false;
            var response = new BarResponse();
            var currentDateTime = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);
            var getBarById = (await _unitOfWork.BarRepository.GetAsync(filter: a => a.BarId == barId,
                    includeProperties: "Feedbacks.Account,BarTimes")).FirstOrDefault();

            if (getBarById == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy quán bar !");
            }

            var tables = await _unitOfWork.TableRepository
                                                .GetAsync(t => t.IsDeleted == false);

            foreach (var table in tables)
            {
                var reservations = await _unitOfWork.BookingTableRepository
                    .GetAsync(filter: bt => bt.TableId == table.TableId &&
                        (bt.Booking.BookingDate + bt.Booking.BookingTime) >= currentDateTime &&
                        (bt.Booking.Status == (int)PrefixValueEnum.Pending || bt.Booking.Status == (int)PrefixValueEnum.Serving),
                        includeProperties: "Booking");

                if (!reservations.Any())
                {
                    isAnyTableAvailable = true;
                    break;
                }
            }

            response = _mapper.Map<BarResponse>(getBarById);
            response.IsAnyTableAvailable = isAnyTableAvailable;
            response.BarTimeResponses = _mapper.Map<List<BarTimeResponse>>(getBarById.BarTimes);
            return response;
        }

        public async Task<BarResponse> GetBarByIdWithTable(Guid barId)
        {
            var getBarById = (await _unitOfWork.BarRepository.GetAsync(filter: a => a.BarId == barId,
                    includeProperties: "TableTypes.Tables,BarTimes")).FirstOrDefault();

            if (getBarById == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy quán bar !");
            }

            var response = _mapper.Map<BarResponse>(getBarById);
            return response;
        }

        public async Task<OnlyBarResponse> UpdateBarById(UpdateBarRequest request)
        {
            var response = new OnlyBarResponse();
            string imageUrl = null;
            string imgsUploaed = string.Empty;
            List<string> imgsList = new List<string>();
            List<IFormFile> imgsUpload = new List<IFormFile>();

            using (TransactionScope transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var getBarById = (await _unitOfWork.BarRepository
                                                            .GetAsync(filter: x => x.BarId.Equals(request.BarId),
                                                                includeProperties: "BarTimes"))
                                                            .FirstOrDefault();

                    if (getBarById == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy quán bar muốn thay đổi !");
                    }

                    var isBarName = _unitOfWork.BarRepository.Get(filter: x => x.BarName.Contains(request.BarName)).ToList();

                    if (isBarName.Count > 1)
                    {
                        throw new CustomException.InvalidDataException("Tên quán Bar đã tồn tại, vui lòng thử lại !");
                    }

                    if (!request.Images.IsNullOrEmpty())
                    {
                        var images = Utils.ConvertBase64ListToFiles(request.Images);
                        imgsUpload = Utils.CheckValidateImageFile(images);
                    }

                    if (request.imgsAsString.IsNullOrEmpty() && getBarById.Images.IsNullOrEmpty())
                    {
                        throw new CustomException.InvalidDataException("Dữ liệu không hợp lệ, vui lòng thử lại !");
                    }

                    foreach (var isValidTimeSlot in request.UpdateBarTimeRequests)
                    {
                        var isValid = Utils.IValidSlot(request.TimeSlot, isValidTimeSlot.StartTime, isValidTimeSlot.EndTime);
                        if (!isValid)
                        {
                            throw new CustomException.InvalidDataException($" Thời gian đóng mở cửa không đủ cho slot vào {Utils.GetDayName(isValidTimeSlot.DayOfWeek)}!");
                        }
                    }
                    _mapper.Map(request, getBarById);
                    getBarById.Images = "";
                    await _unitOfWork.BarRepository.UpdateAsync(getBarById);

                    await _barTimeService.UpdateBarTimeOfBar(getBarById.BarId, request.UpdateBarTimeRequests);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();


                    foreach (var image in imgsUpload)
                    {
                        imageUrl = await _fireBase.UploadImageAsync(image);
                        imgsList.Add(imageUrl);
                    }



                    var imgsAsString = string.Join(",", imgsList);
                    imgsUploaed = string.Join(",", request.imgsAsString);

                    var allImg = string.IsNullOrEmpty(imgsAsString) ? imgsUploaed : $"{imgsUploaed},{imgsAsString}";

                    getBarById.Images = allImg;
                    await _unitOfWork.BarRepository.UpdateAsync(getBarById);
                    await Task.Delay(200);
                    await _unitOfWork.SaveAsync();

                    transaction.Complete();

                    response = _mapper.Map<OnlyBarResponse>(getBarById);
                    response.BarTimeResponses = _mapper.Map<List<BarTimeResponse>>(getBarById.BarTimes);
                }
                catch (CustomException.InternalServerErrorException e)
                {
                    transaction.Dispose();
                    throw new CustomException.InternalServerErrorException(e.Message);
                }
            }
            return response;
        }

        public async Task<IEnumerable<OnlyBarResponse>> GetAllAvailableBars(DateTime dateTime)
        {
            try
            {
                var bars = await _unitOfWork.BarRepository
                    .GetAsync(filter: x => x.BarTimes.Any(x => x.DayOfWeek == (int)dateTime.DayOfWeek)
                            && x.Status == true,
                        includeProperties: "Feedbacks,BarTimes");
                var currentDateTime = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);
                var response = new List<OnlyBarResponse>();

                if (bars.IsNullOrEmpty() || !bars.Any())
                {
                    throw new CustomException.DataNotFoundException("Danh sách đang trống !");
                }

                foreach (var bar in bars)
                {
                    var tables = await _unitOfWork.TableRepository
                                                    .GetAsync(t => t.IsDeleted == false);
                    bool isAnyTableAvailable = false;
                    foreach (var table in tables)
                    {
                        var reservations = await _unitOfWork.BookingTableRepository
                            .GetAsync(filter: bt => bt.TableId == table.TableId &&
                            (bt.Booking.BookingDate + bt.Booking.BookingTime) >= currentDateTime &&
                            (bt.Booking.Status == (int)PrefixValueEnum.Pending || bt.Booking.Status == (int)PrefixValueEnum.Serving),
                            includeProperties: "Booking");

                        if (!reservations.Any())
                        {
                            isAnyTableAvailable = true;
                            break;
                        }
                    }
                    var mapper = _mapper.Map<OnlyBarResponse>(bar);
                    mapper.IsAnyTableAvailable = isAnyTableAvailable;
                    mapper.BarTimeResponses = _mapper.Map<List<BarTimeResponse>>(bar.BarTimes);
                    response.Add(mapper);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<RevenueResponse> GetRevenueOfBar(RevenueRequest request)
        {
            try
            {
               
                DateTime? targetDate = request.DateTime;

                if(targetDate != null && request.Type.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Đã nhập ngày thì vui lòng nhập type bạn muốn filter !");
                }

                if (targetDate == null && !request.Type.IsNullOrEmpty())
                {
                    throw new CustomException.InvalidDataException("Đã nhập type thì vui lòng nhập ngày bạn muốn filter !");
                }

                if (request.BarId != null)
                {
                    var IsBarExist = await _unitOfWork.BarRepository.GetByIdAsync(Guid.Parse(request.BarId));
                    if (IsBarExist == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy quán bar !");
                    }
                }

                var bookings = await _unitOfWork.BookingRepository.GetAsync(
                                        filter: x =>
                                            x.Status == (int)PrefixValueEnum.Completed &&
                                            (request.BarId == null || x.BarId == Guid.Parse(request.BarId)) &&
                                            (!targetDate.HasValue || (
                                                (request.Type != null && request.Type.ToLower() == "day" && 
                                                    x.BookingDate.Date == targetDate.Value.Date) ||
                                                (request.Type != null && request.Type.ToLower() == "month" && 
                                                    x.BookingDate.Year == targetDate.Value.Year && 
                                                    x.BookingDate.Month == targetDate.Value.Month) ||
                                                (request.Type != null && request.Type.ToLower() == "year" && 
                                                    x.BookingDate.Year == targetDate.Value.Year)
                                            ))
                                    );

                var totalRevenue = bookings.Sum(b => b.TotalPrice + b.AdditionalFee);

                return new RevenueResponse
                {
                    RevenueOfBar = totalRevenue ?? 0,
                    BarId = request.BarId ?? null,
                };
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<OnlyIdNameResponse>> GetBarNameId(ObjectQuery query)
        {
            Expression<Func<Bar, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                filter = bar => bar.BarName.Contains(query.Search);
            }

            var getAllBar = await _unitOfWork.BarRepository
                                .GetAsync(filter: filter,
                                pageIndex: query.PageIndex,
                                pageSize: query.PageSize);

            if (getAllBar.IsNullOrEmpty() || !getAllBar.Any())
            {
                throw new CustomException.DataNotFoundException("Danh sách đang trống !");
            }

            var response = _mapper.Map<List<OnlyIdNameResponse>>(getAllBar);
            return response;
        }
    }
}
