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
using Application.DTOs.Booking;
using Azure.Core;
using MediatR;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;

namespace Application.Service
{
    public class BarService : IBarService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFirebase _fireBase;
        private readonly IBarTimeService _barTimeService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthentication _authentication;
        public BarService(IUnitOfWork unitOfWork, IMapper mapper,
                            IFirebase fireBase, IBarTimeService barTimeService,
                            IAuthentication authentication,
                            IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBase = fireBase;
            _barTimeService = barTimeService;
            _authentication = authentication;
            _contextAccessor = contextAccessor;
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

        public async Task<PagingBarResponse> GetAllBar(ObjectQueryCustom query)
        {
            Expression<Func<Bar, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                filter = bar => bar.BarName.Contains(query.Search);
            }

            var getAllBar = await _unitOfWork.BarRepository
                                            .GetAsync(filter: filter,
                                                orderBy: query => query.OrderBy(x => x.BarName),
                                                includeProperties: "BarTimes");

            if (getAllBar.IsNullOrEmpty() || !getAllBar.Any())
            {
                throw new CustomException.DataNotFoundException("Danh sách đang trống !");
            }

            var response = _mapper.Map<List<BarResponse>>(getAllBar);
            foreach (var barTime in response)
            {
                var getOneBar = getAllBar.Where(x => x.BarId.Equals(barTime.BarId)).FirstOrDefault();
                barTime.BarTimeResponses = _mapper.Map<List<BarTimeResponse>>(getOneBar?.BarTimes);
            }

            var pageIndex = query.PageIndex ?? 1;
            var pageSize = query.PageSize ?? 6;

            var totalItems = response.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedBars = response.Skip((pageIndex - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToList();

            return new PagingBarResponse
            {
                BarResponses = paginatedBars,
                TotalPages = totalPages,
                CurrentPage = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems
            };
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
                                              orderBy: query => query.OrderBy(x => x.BarName),
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

                    var getEventOfBar = _unitOfWork.EventRepository.Get(filter: x => x.Bar.BarId.Equals(getBarById.BarId));
                    
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

                    if (getEventOfBar != null && getEventOfBar.Any())
                    {
                        foreach (var eventItem in getEventOfBar)
                        {
                            eventItem.IsHide = !eventItem.IsHide;
                            await _unitOfWork.EventRepository.UpdateAsync(eventItem);
                        }
                    }
                    await _unitOfWork.BarRepository.UpdateAsync(getBarById);
                    await Task.Delay(20);
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

        public async Task<IEnumerable<OnlyBarResponse>> GetAllAvailableBars(DateTime dateTime, ObjectQuery query)
        {
            try
            {
                Expression<Func<Bar, bool>> filter = x => x.BarTimes.Any(x => x.DayOfWeek == (int)dateTime.DayOfWeek) && x.Status == true;

                if (!string.IsNullOrWhiteSpace(query.Search))
                {
                    filter = bar => bar.BarName.Contains(query.Search) 
                        && bar.BarTimes.Any(x => x.DayOfWeek == (int)dateTime.DayOfWeek) 
                        && bar.Status == true;
                }

                var bars = await _unitOfWork.BarRepository
                    .GetAsync(filter: filter,
                        orderBy: query => query.OrderBy(x => x.BarName),
                        includeProperties: "Feedbacks,BarTimes",
                        pageIndex: query.PageIndex,
                        pageSize: query.PageSize
                        );
                var currentDateTime = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now);
                var response = new List<OnlyBarResponse>();

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
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                if (getAccount.BarId.HasValue &&  request.BarId != null && !getAccount.BarId.Equals(Guid.Parse(request.BarId)))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào Bar này !");
                }
                if ((request.FromTime.HasValue && !request.ToTime.HasValue) ||
                    (!request.FromTime.HasValue && request.ToTime.HasValue))
                {
                    throw new CustomException.InvalidDataException("Vui lòng nhập cả FromTime và ToTime!");
                }

                if (request.FromTime > request.ToTime)
                {
                    throw new CustomException.InvalidDataException("FromTime không thể lớn hơn ToTime!");
                }

                if (request.BarId != null)
                {
                    var IsBarExist = await _unitOfWork.BarRepository.GetByIdAsync(Guid.Parse(request.BarId));
                    if (IsBarExist == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy quán bar!");
                    }
                }

                var bookings = await _unitOfWork.BookingRepository.GetAsync(
                    filter: x =>
                        x.Status != (int)PrefixValueEnum.Serving &&
                        x.Status != (int)PrefixValueEnum.Pending &&
                        (request.BarId == null || x.BarId == Guid.Parse(request.BarId)) &&
                        (!request.FromTime.HasValue || x.BookingDate >= request.FromTime.Value.Date) &&
                        (!request.ToTime.HasValue || x.BookingDate <= request.ToTime.Value.Date),
                    orderBy: x => x.OrderBy(x => x.BookingDate)
                );
                var basedBookingDate = bookings.GroupBy(x => x.BookingDate.Date).ToList();

                var totalRevenue = basedBookingDate.Sum(group =>
                {
                    var total = group.Sum(x => (x.TotalPrice ?? 0) + (x.AdditionalFee ?? 0));
                    return total;
                });

                return new RevenueResponse
                {
                    RevenueOfBar = totalRevenue,
                    BarId = request.BarId ?? null,
                    TotalBooking = bookings.Count(),
                    FromTime = request.FromTime ?? DateTime.MinValue,
                    ToTime = request.ToTime ?? DateTime.MinValue,
                    BookingReveueResponses = basedBookingDate.Select(x => new BookingReveueResponse
                    {
                        Date = x.Key.Date,
                        TotalBookingOfDay = x.Count(),
                        TotalPrice = x.Sum(x => (x.TotalPrice ?? 0) + (x.AdditionalFee ?? 0))
                    }).ToList(),
                };
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<OnlyBarIdNameResponse>> GetBarNameId(ObjectQuery query)
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

            var response = _mapper.Map<List<OnlyBarIdNameResponse>>(getAllBar);
            return response;
        }

        public async Task<RevenueBranchResponse> GetAllRevenueBranch()
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var bookings = await _unitOfWork.BookingRepository.GetAsync(
                    filter: x =>
                        x.Status != (int)PrefixValueEnum.Serving &&
                        x.Status != (int)PrefixValueEnum.Pending &&
                        (!getAccount.BarId.HasValue || x.BarId.Equals(getAccount.BarId)),
                    orderBy: x => x.OrderBy(x => x.BookingDate)
                );

                var barList = await _unitOfWork.BarRepository.GetAllAsync();

                var totalRevenue = bookings.Sum(x => (x.TotalPrice ?? 0) + (x.AdditionalFee ?? 0));
                var totalBooking = bookings.Count();
                var totalBar = barList.Where( x => !getAccount.BarId.HasValue || x.BarId.Equals(getAccount.BarId)).Count();

                var response = new RevenueBranchResponse
                {
                    RevenueBranch = totalRevenue,
                    TotalBarBranch = totalBar,
                    TotalBooking = totalBooking,
                };

                return response;
            }
            catch(CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task<PagingOnlyBarIdNameResponse> GetBarNameIdAdMa(ObjectQuery query)
        {
            var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
            var getAccount = _unitOfWork.AccountRepository
                                        .Get(filter: x => x.AccountId.Equals(accountId),
                                             includeProperties: "Role").FirstOrDefault();

            Expression<Func<Bar, bool>> filter = null;
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                if (getAccount.Role.RoleName.Equals(PrefixKeyConstant.MANAGER) || 
                    getAccount.Role.RoleName.Equals(PrefixKeyConstant.STAFF))
                {
                    filter = bar => bar.BarId.Equals(getAccount.BarId) &&
                                   bar.BarName.Contains(query.Search);
                }
                else
                {
                    filter = bar => bar.BarName.Contains(query.Search);
                }
            }
            else
            {
                if (getAccount.Role.RoleName.Equals(PrefixKeyConstant.MANAGER))
                {
                    filter = bar => bar.BarId.Equals(getAccount.BarId);
                }
            }

            var bars = (await _unitOfWork.BarRepository.GetAsync(filter: filter))
                        .ToList();

            if (!bars.Any())
            {
                throw new CustomException.DataNotFoundException("Danh sách đang trống !");
            }

            var response = _mapper.Map<List<OnlyBarNameIdResponse>>(bars);

            var pageIndex = query.PageIndex ?? 1;
            var pageSize = query.PageSize ?? 6;

            int validPageIndex = pageIndex > 0 ? pageIndex - 1 : 0;
            int validPageSize = pageSize > 0 ? pageSize : 10;

            var totalItems = response.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)validPageSize);

            var paginatedBars = response.Skip(validPageIndex * validPageSize)
                                      .Take(validPageSize)
                                      .ToList();

            return new PagingOnlyBarIdNameResponse
            {
                OnlyBarIdNameResponses = paginatedBars,
                TotalPages = totalPages,
                CurrentPage = pageIndex,
                PageSize = validPageSize,
                TotalItems = totalItems
            };
        }

        public async Task<(byte[] fileContents, string fileName)> DownloadRevenueExcel(ObjectKeyDateTimeQuery query)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                if (getAccount.BarId.HasValue && query.BarId != null && !getAccount.BarId.Equals(query.BarId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào Bar này !");
                }
                if ((query.FromDate.HasValue && !query.ToDate.HasValue) ||
                    (!query.FromDate.HasValue && query.ToDate.HasValue))
                {
                    throw new CustomException.InvalidDataException("Vui lòng nhập cả FromDate và ToDateTime!");
                }

                if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate.Value > query.ToDate.Value)
                {
                    throw new CustomException.InvalidDataException("FromDate không thể lớn hơn ToDateTime!");
                }
                var bookings = await _unitOfWork.BookingRepository.GetAsync(
                    filter: x => (x.BarId.Equals(getAccount.BarId) || x.BarId == null) && 
                                x.Status != (int)PrefixValueEnum.Serving &&
                                x.Status != (int)PrefixValueEnum.Pending &&
                                (!query.FromDate.HasValue || x.BookingDate.Date >= query.FromDate.Value.Date) &&
                                (!query.ToDate.HasValue || x.BookingDate.Date <= query.ToDate.Value.Date),
                    orderBy: x => x.OrderBy(x => x.BookingDate),
                    includeProperties: "Account,BookingTables,Bar"
                );

                var getBarName = bookings.Select(x => x.Bar.BarName).FirstOrDefault();
                
                var assembly = typeof(Domain.Common.BaseEntity).Assembly;
                var resourceNames = assembly.GetManifestResourceNames();
                foreach (var name in resourceNames)
                {
                    Console.WriteLine($"Resource found: {name}");
                }

                var stream = assembly.GetManifestResourceStream("Domain.Common.ExcelFile.Revenue.xlsx");
                if (stream == null)
                {
                    var resourceName = resourceNames.FirstOrDefault(x => x.EndsWith("Revenue.xlsx"));
                    if (resourceName != null)
                    {
                        stream = assembly.GetManifestResourceStream(resourceName);
                    }
                    else
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy file template!");
                    }
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                var reportTimeCell = worksheet.Cells[15, 2, 15, 18];  // Dòng 15, từ cột B đến R
                reportTimeCell.Merge = true;
                reportTimeCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                string reportTimeText;
                if (query.FromDate.HasValue && query.ToDate.HasValue)
                {
                    reportTimeText = $"Thời gian báo cáo: {query.FromDate.Value.ToString("dd/MM/yyyy")} - {query.ToDate.Value.ToString("dd/MM/yyyy")}";
                }
                else
                {
                    reportTimeText = $"Thời gian báo cáo: Tất cả";
                }
                reportTimeCell.Value = reportTimeText;

                int startRow = 17;
                int currentRow = startRow;
                double totalRevenue = 0;
                int totalBookings = 0;

                foreach (var booking in bookings)
                {
                    var bookingTotal = booking.TotalPrice ?? 0;
                    var additionalFee = booking.AdditionalFee ?? 0;
                    var grandTotal = bookingTotal + additionalFee;

                    totalRevenue += grandTotal;
                    totalBookings++;

                    var mergePairs = new[]
                    {
                        worksheet.Cells[currentRow, 2, currentRow, 3],
                        worksheet.Cells[currentRow, 4, currentRow, 5],
                        worksheet.Cells[currentRow, 9, currentRow, 10],
                        worksheet.Cells[currentRow, 11, currentRow, 12],
                        worksheet.Cells[currentRow, 13, currentRow, 14],
                        worksheet.Cells[currentRow, 17, currentRow, 18]
                    };

                    foreach (var mergeRange in mergePairs)
                    {
                        mergeRange.Merge = true;
                        mergeRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    var rowRange = worksheet.Cells[currentRow, 2, currentRow, 18];
                    rowRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    rowRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    rowRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    rowRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    rowRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rowRange.Style.Font.Bold = false;
                    rowRange.Style.Font.Size = 11;

                    // Thêm màu cho trạng thái đơn
                    var statusCell = worksheet.Cells[currentRow, 16];  // Cột trạng thái đơn
                    if (booking.Status == 3) // Hoàn thành
                    {
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(198, 239, 206)); // Màu xanh nhạt
                        statusCell.Style.Font.Color.SetColor(Color.FromArgb(0, 97, 0));  // Màu xanh đậm cho chữ
                    }
                    else // Đã hủy
                    {
                        statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 199, 206)); // Màu đỏ nhạt
                        statusCell.Style.Font.Color.SetColor(Color.FromArgb(156, 0, 6));  // Màu đỏ đậm cho chữ
                    }

                    worksheet.Cells[currentRow, 2].Value = booking.BookingDate.ToString("dd/MM/yyyy");
                    worksheet.Cells[currentRow, 4].Value = booking.BookingTime;
                    worksheet.Cells[currentRow, 6].Value = booking.Account.Email;
                    worksheet.Cells[currentRow, 7].Value = booking.Account.Phone;
                    worksheet.Cells[currentRow, 8].Value = booking.BookingCode;
                    worksheet.Cells[currentRow, 9].Value = booking.BookingTables.Count();
                    worksheet.Cells[currentRow, 11].Value = bookingTotal;
                    worksheet.Cells[currentRow, 13].Value = additionalFee;
                    worksheet.Cells[currentRow, 15].Value = bookingTotal == 0 ? "Không" : "Chuyển khoản";
                    worksheet.Cells[currentRow, 16].Value = booking.Status == 3 ? "Hoàn thành" : "Đã hủy";
                    worksheet.Cells[currentRow, 17].Value = grandTotal;

                    currentRow++;
                }

                // Add total row
                var totalRow = currentRow;
                var totalRowRange = worksheet.Cells[totalRow, 2, totalRow, 18];

                // Style cho toàn bộ dòng tổng cộng
                totalRowRange.Style.Font.Bold = true;
                totalRowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                totalRowRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                totalRowRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                totalRowRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                totalRowRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                totalRowRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                totalRowRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Merge các cột theo yêu cầu
                worksheet.Cells[totalRow, 2, totalRow, 7].Merge = true;  // Merge B-G cho "Tổng cộng"
                worksheet.Cells[totalRow, 9, totalRow, 10].Merge = true; // Merge I-J
                worksheet.Cells[totalRow, 11, totalRow, 12].Merge = true; // Merge K-L cho tổng tiền
                worksheet.Cells[totalRow, 13, totalRow, 14].Merge = true; // Merge M-N cho tổng phụ thu
                worksheet.Cells[totalRow, 17, totalRow, 18].Merge = true; // Merge Q-R cho tổng doanh thu

                // Tính tổng tiền và phụ thu
                var totalPrice = bookings.Sum(x => x.TotalPrice ?? 0);
                var totalAdditionalFee = bookings.Sum(x => x.AdditionalFee ?? 0);
                var totalOrders = bookings.Count();  // Tổng số đơn ở cột H

                // Gán giá trị
                worksheet.Cells[totalRow, 2].Value = "Tổng cộng";
                worksheet.Cells[totalRow, 8].Value = totalOrders;     // Tổng số đơn ở cột H
                worksheet.Cells[totalRow, 9].Value = totalBookings;   // Tổng số lượng booking
                worksheet.Cells[totalRow, 11].Value = totalPrice;     // Tổng tiền
                worksheet.Cells[totalRow, 13].Value = totalAdditionalFee;  // Tổng phụ thu
                worksheet.Cells[totalRow, 17].Value = totalRevenue;   // Tổng doanh thu

                string fileName = $"DoanhThu_{getBarName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return (package.GetAsByteArray(), fileName);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException($"Lỗi khi tạo file Excel: {ex.Message}");
            }
        }
    }
}
