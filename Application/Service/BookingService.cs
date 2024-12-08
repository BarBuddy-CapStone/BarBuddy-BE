using Application.Common;
using Application.DTOs.Booking;
using Application.DTOs.BookingDrink;
using Application.DTOs.BookingExtraDrink;
using Application.DTOs.BookingTable;
using Application.DTOs.Events.EventVoucher;
using Application.DTOs.Fcm;
using Application.DTOs.Notification;
using Application.DTOs.Payment;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Domain.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Domain.CustomException.CustomException;

namespace Application.Service
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthentication _authentication;
        private readonly IPaymentService _paymentService;
        private readonly IEmailSender _emailSender;
        private readonly IQRCodeService _qrCodeService;
        private readonly IFirebase _firebase;
        private readonly ILogger<BookingService> _logger;
        private readonly IEventVoucherService _eventVoucherService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IFcmService _fcmService;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IAuthentication authentication,
            IPaymentService paymentService, IEmailSender emailSender, IQRCodeService qrCodeService, IFirebase firebase,
            IEventVoucherService eventVoucherService, IHttpContextAccessor contextAccessor, IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authentication = authentication;
            _paymentService = paymentService;
            _emailSender = emailSender;
            _qrCodeService = qrCodeService;
            _firebase = firebase;
            _eventVoucherService = eventVoucherService;
            _contextAccessor = contextAccessor;
            _fcmService = fcmService;
        }

        public async Task<bool> CancelBooking(Guid BookingId)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var booking = (await _unitOfWork.BookingRepository
                                                .GetAsync(b => b.BookingId == BookingId &&
                                                               b.AccountId.Equals(accountId),
                                                               includeProperties: "Account,Bar"))
                                                .FirstOrDefault();

                if (booking == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy Id Đặt bàn.");
                }

                if (!getAccount.AccountId.Equals(booking.AccountId))
                {
                    throw new UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (booking.Status != 0)
                {
                    throw new CustomException.DataNotFoundException("Bạn không thể hủy đặt bàn.");
                }

                DateTime BookingDateTime = booking.BookingDate.Date + booking.BookingTime;
                if (BookingDateTime < DateTime.Now.AddHours(2))
                {
                    return false;
                }


                // Cancelled status (temp)
                booking.Status = 1;
                booking.CheckOutStaffId = accountId;
                _unitOfWork.BeginTransaction();
                await _unitOfWork.BookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveAsync();
                _unitOfWork.CommitTransaction();

                var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);

                //var fcmNotification = new CreateNotificationRequest
                //{
                //    BarId = booking.Bar.BarId,
                //    DeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                //    ImageUrl = bar == null ? null : bar.Images.Split(',')[0],
                //    IsPublic = false,
                //    Message = $"Đơn đặt chỗ tại {bar.BarName} với mã đặt chỗ {booking.BookingCode} vào lúc {booking.BookingTime} - {booking.BookingDate.ToString("yyyy/mm/dd")} đã được hủy thành công.",
                //    Title = $"Hủy đặt chỗ tại {bar.BarName} thành công!",
                //    Type = FcmNotificationType.BOOKING
                //};

                //await _fcmService.CreateAndSendNotificationToCustomer(fcmNotification, booking.AccountId);

                List<Guid> ids = new List<Guid>();
                ids.Add(booking.AccountId);

                var fcmNotification = new CreateNotificationRequest
                {
                    BarId = booking.Bar.BarId,
                    MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                    WebDeepLink = $"booking-detail/{booking.BookingId}",
                    ImageUrl = bar == null ? null : bar.Images.Split(',')[0],
                    IsPublic = false,
                    Message = $"Đơn đặt chỗ tại {bar.BarName} với mã đặt chỗ {booking.BookingCode} vào lúc {booking.BookingTime} - {booking.BookingDate.ToString("yyyy/mm/dd")} đã được hủy thành công.",
                    Title = $"Hủy đặt chỗ tại {bar.BarName} thành công!",
                    Type = FcmNotificationType.BOOKING,
                    SpecificAccountIds = ids
                };

                await _fcmService.CreateAndSendNotification(fcmNotification);

                return true;
            }
            catch (DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<TopBookingResponse> responses, int TotalPage)> GetAllCustomerBooking(Guid CustomerId, int? Status, int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);

                if (!accountId.Equals(CustomerId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào tài khoản này !");
                }
                var responses = new List<TopBookingResponse>();
                int TotalPage = 1;

                var bookings = await _unitOfWork.BookingRepository.GetAsync(b => b.AccountId == CustomerId && (Status == null || b.Status == Status));

                if (bookings.Count() > PageSize)
                {
                    if (PageSize == 1)
                    {
                        TotalPage = (bookings.Count() / PageSize);
                    }
                    else
                    {
                        TotalPage = (bookings.Count() / PageSize) + 1;
                    }
                }

                var bookingsWithPagination = await _unitOfWork.BookingRepository.GetAsync(b => b.AccountId == CustomerId && (Status == null || b.Status == Status), includeProperties: "Bar", pageSize: PageSize, pageIndex: PageIndex, orderBy: o => o.OrderByDescending(b => b.CreateAt).ThenByDescending(b => b.BookingDate));

                foreach (var booking in bookingsWithPagination)
                {
                    var feedback = await _unitOfWork.FeedbackRepository.GetAsync(f => f.BookingId == booking.BookingId);
                    bool? checkIsRated = null;
                    if (booking.Status == 3)
                    {
                        if (feedback.Any())
                        {
                            checkIsRated = true;
                        }
                        else
                        {
                            checkIsRated = false;
                        }
                    }
                    var response = new TopBookingResponse
                    {
                        BarName = booking.Bar.BarName,
                        BarId = booking.BarId,
                        BookingDate = booking.BookingDate,
                        BookingId = booking.BookingId,
                        BookingTime = booking.BookingTime,
                        CreateAt = booking.CreateAt,
                        Status = booking.Status,
                        Image = booking.Bar.Images.Split(',')[0],
                        IsRated = checkIsRated,
                        BookingCode = booking.BookingCode,
                        Note = booking.Note
                    };
                    responses.Add(response);
                }

                return (responses, TotalPage);
            }
            catch (InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<BookingByIdResponse> GetBookingById(Guid BookingId)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);

                var response = new BookingByIdResponse();

                var booking = (await _unitOfWork.BookingRepository
                                                .GetAsync(b => b.BookingId == BookingId &&
                                                               b.AccountId.Equals(accountId),
                                                          includeProperties: "Account,Bar,BookingExtraDrinks.Drink,BookingDrinks.Drink"))
                                                .FirstOrDefault();

                if (booking == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy đơn đặt bàn");
                }

                bool? isRated = null;

                if (booking.Status == 3)
                {
                    var feedback = (await _unitOfWork.FeedbackRepository.GetAsync(f => f.BookingId == booking.BookingId)).FirstOrDefault();
                    if (feedback != null)
                    {
                        isRated = true;
                    }
                    else
                    {
                        isRated = false;
                    }
                }

                response.BookingId = booking.BookingId;
                response.BookingTime = booking.BookingTime;
                response.BarId = booking.BarId;
                response.Status = booking.Status;
                response.CreateAt = booking.CreateAt;
                response.BookingDate = booking.BookingDate;
                response.BarName = booking.Bar.BarName;
                response.BarAddress = booking.Bar.Address;
                response.CustomerPhone = booking.Account.Phone;
                response.CustomerName = booking.Account.Fullname;
                response.CustomerEmail = booking.Account.Email;
                response.BookingCode = booking.BookingCode;
                response.AdditionalFee = booking.AdditionalFee;
                response.TotalPrice = booking.TotalPrice;
                response.Note = booking.Note;
                response.QrTicket = booking.QRTicket;
                response.IsRated = isRated;
                response.Images = booking.Bar.Images.Split(',').ToList();
                response.NumOfPeople = booking.NumOfPeople;
                response.NumOfTable = booking.NumOfTable;

                if (booking.TotalPrice >= 0 || (booking.AdditionalFee >= 0 && booking.AdditionalFee.HasValue))
                {
                    response.bookingDrinksList = _mapper.Map<List<BookingDrinkDetailResponse>>(booking.BookingDrinks);
                    response.BookingDrinkExtraResponses = _mapper.Map<List<BookingDrinkExtraResponse>>(booking.BookingExtraDrinks);
                    foreach (var drink in response.BookingDrinkExtraResponses)
                    {
                        if (drink.StaffId != null)
                        {
                            var staff = await _unitOfWork.AccountRepository.GetByIdAsync(drink.StaffId);
                            drink.StaffName = staff != null ? staff.Fullname : "Nhân viên";
                        } else
                        {
                            drink.StaffName = "Nhân viên";
                        }
                    }
                }

                var bookingTables = await _unitOfWork.BookingTableRepository.GetAsync(bt => bt.BookingId == BookingId, includeProperties: "Table");
                foreach (var table in bookingTables)
                {
                    response.TableNameList.Add(table.Table.TableName);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<BookingDetailByStaff> GetBookingDetailAuthorized(Guid BookingId)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var response = new BookingDetailByStaff();

                var booking = (await _unitOfWork.BookingRepository
                                                .GetAsync(b => b.BookingId == BookingId &&
                                                               b.BarId.Equals(getAccount.BarId),
                                                          includeProperties: "Account,Bar,BookingExtraDrinks.Drink,BookingDrinks.Drink"))
                                                .FirstOrDefault();

                if (!getAccount.BarId.Equals(booking?.BarId))
                {
                    throw new UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (booking == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy Id Đặt bàn");
                }


                response.BookingId = booking.BookingId;
                response.BookingTime = booking.BookingTime;
                response.Status = booking.Status;
                response.CreateAt = booking.CreateAt;
                response.BookingDate = booking.BookingDate;
                response.BarName = booking.Bar.BarName;
                response.BarAddress = booking.Bar.Address;
                response.CustomerPhone = booking.Account.Phone;
                response.CustomerName = booking.Account.Fullname;
                response.CustomerEmail = booking.Account.Email;
                response.BookingCode = booking.BookingCode;
                response.AdditionalFee = booking.AdditionalFee;
                response.TotalPrice = booking.TotalPrice;
                response.Note = booking.Note;
                response.QRTicket = booking.QRTicket;
                response.NumOfPeople = booking.NumOfPeople;
                response.NumOfTable = booking.NumOfTable;

                if (booking.TotalPrice >= 0 || (booking.AdditionalFee >= 0 && booking.AdditionalFee.HasValue))
                {
                    response.bookingDrinksList = _mapper.Map<List<BookingDrinkDetailResponse>>(booking.BookingDrinks);
                    response.BookingDrinkExtraResponses = _mapper.Map<List<BookingDrinkExtraResponse>>(booking.BookingExtraDrinks);
                }

                var bookingTables = await _unitOfWork.BookingTableRepository.GetAsync(bt => bt.BookingId == BookingId);
                foreach (var table in bookingTables)
                {
                    var Tables = (await _unitOfWork.TableRepository.GetAsync(t => t.TableId == table.TableId, includeProperties: "TableType")).FirstOrDefault();
                    var BookingTable = new BookingTableResponseByStaff
                    {
                        TableName = Tables.TableName,
                        TableTypeName = Tables.TableType.TypeName
                    };
                    response.bookingTableList.Add(BookingTable);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<StaffBookingReponse> responses, int TotalPage)> GetListBookingAuthorized(string qrTicket, Guid BarId, string? CustomerName, string? Phone, string? Email, DateTimeOffset? bookingDate, TimeSpan? bookingTime, int? Status, int PageIndex, int PageSize)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                if (!getAccount.BarId.Equals(BarId))
                {
                    throw new UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                var responses = new List<StaffBookingReponse>();

                var Bar = await _unitOfWork.BarRepository.GetByIdAsync(BarId);
                if (Bar == null)
                {
                    throw new CustomException.DataNotFoundException("Không có thông tin của Bar");
                }

                Expression<Func<Booking, bool>> filter = b =>
                b.BarId == BarId &&
                (string.IsNullOrEmpty(CustomerName) || b.Account.Fullname.Contains(CustomerName)) &&
                (string.IsNullOrEmpty(Phone) || b.Account.Phone.Contains(Phone)) &&
                (string.IsNullOrEmpty(Email) || b.Account.Email.Contains(Email)) &&
                (!bookingDate.HasValue || b.BookingDate.Date.Day == bookingDate.Value.Date.Day) &&
                (!bookingTime.HasValue || b.BookingTime == bookingTime.Value) &&
                (!Status.HasValue || b.Status == Status.Value) &&
                (string.IsNullOrEmpty(qrTicket) || b.BookingId.Equals(Guid.Parse(qrTicket)));

                var bookings = await _unitOfWork.BookingRepository.GetAsync(filter);

                int totalPage = 1;
                if (bookings.Count() > PageSize)
                {
                    if (PageSize == 1)
                    {
                        totalPage = (bookings.Count() / PageSize);
                    }
                    else
                    {
                        totalPage = (bookings.Count() / PageSize) + 1;
                    }
                }

                var bookingsWithPagination = await _unitOfWork.BookingRepository.GetAsync(filter: filter, includeProperties: "Account,Bar", pageIndex: PageIndex, pageSize: PageSize, orderBy: o => o.OrderByDescending(b => b.BookingDate).ThenByDescending(b => b.BookingTime));

                foreach (var booking in bookingsWithPagination)
                {
                    var response = new StaffBookingReponse
                    {
                        BookingDate = booking.BookingDate,
                        BookingId = booking.BookingId,
                        BookingTime = booking.BookingTime,
                        CustomerName = booking.Account.Fullname,
                        Email = booking.Account.Email,
                        Phone = booking.Account.Phone,
                        Status = booking.Status,
                        BookingCode = booking.BookingCode,
                        AdditionalFee = booking.AdditionalFee,
                        TotalPrice = booking.TotalPrice,
                        QRTicket = booking.QRTicket,
                        Note = booking.Note,
                        NumOfPeople = booking.NumOfPeople,
                        NumOfTable = booking.NumOfTable
                    };
                    responses.Add(response);
                }
                return (responses, totalPage);
            }
            catch (DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (UnAuthorizedException ex)
            {
                throw new CustomException.UnAuthorizedException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<TopBookingResponse>> GetTopBookingByCustomer(Guid CustomerId, int NumOfBookings)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                if (!accountId.Equals(CustomerId))
                {
                    throw new UnAuthorizedException("Bạn không có quyền truy cập vào tài khoản này !");
                }
                var responses = new List<TopBookingResponse>();

                var bookings = await _unitOfWork.BookingRepository.GetAsync(b => b.AccountId == accountId,
                    pageIndex: 1, pageSize: NumOfBookings,
                    orderBy: o => o.OrderByDescending(b => b.CreateAt).ThenByDescending(b => b.BookingDate),
                    includeProperties: "Bar");
                foreach (var booking in bookings)
                {
                    var feedback = await _unitOfWork.FeedbackRepository.GetAsync(f => f.BookingId == booking.BookingId);
                    bool? checkIsRated = null;
                    if (booking.Status == 3)
                    {
                        if (feedback.Any())
                        {
                            checkIsRated = true;
                        }
                        else
                        {
                            checkIsRated = false;
                        }
                    }
                    var response = new TopBookingResponse
                    {
                        BarName = booking.Bar.BarName,
                        BookingDate = booking.BookingDate,
                        BookingId = booking.BookingId,
                        BookingTime = booking.BookingTime,
                        CreateAt = booking.CreateAt,
                        Status = booking.Status,
                        Image = booking.Bar.Images.Split(',')[0],
                        IsRated = checkIsRated,
                        BookingCode = booking.BookingCode,
                        Note = booking.Note
                    };
                    responses.Add(response);
                }

                return responses;
            }
            catch (InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<BookingResponse> CreateBookingTableOnly(BookingTableRequest request, HttpContext httpContext)
        {
            try
            {
                var booking = _mapper.Map<Booking>(request);
                if (request?.TableIds?.Count() > 6)
                {
                    throw new CustomException.InvalidDataException("Chỉ được đặt tối đa 5 bàn");
                }
                await ValidateTableAvailability(request.TableIds, request.BookingDate, request.BookingTime, request.BarId);

                booking.Account = _unitOfWork.AccountRepository.GetByID(_authentication.GetUserIdFromHttpContext(httpContext))
                    ?? throw new DataNotFoundException("Không tìm thấy tài khoản !");
                booking.Bar = _unitOfWork.BarRepository.GetByID(request.BarId)
                    ?? throw new DataNotFoundException("Không tìm thấy quán bar !");

                var barTimes = await _unitOfWork.BarTimeRepository.GetAsync(x => x.BarId == request.BarId);
                if (barTimes == null || !barTimes.Any())
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy thông tin thời gian của Bar.");
                }

                Utils.ValidateOpenCloseTimeWithTimeSlot(request.BookingDate, request.BookingTime,
                    barTimes.ToList(), booking.Bar.TimeSlot);

                booking.BookingTables = booking.BookingTables ?? new List<BookingTable>();
                booking.BookingCode = $"{booking.BookingDate.ToString("yyMMdd")}{RandomHelper.GenerateRandomNumberString()}";
                booking.Status = (int)PrefixValueEnum.PendingBooking;
                var qrCode = _qrCodeService.GenerateQRCode(booking.BookingId);
                booking.QRTicket = await _firebase.UploadImageAsync(Utils.ConvertBase64ToFile(qrCode));
                booking.BookingTime = request.BookingTime;
                booking.ExpireAt = (request.BookingDate + request.BookingTime).AddHours(2);
                booking.TotalPrice = null;
                booking.NumOfTable = request?.TableIds?.Count ?? 0;
                if (request?.TableIds != null && request.TableIds.Count > 0)
                {
                    var existingTables = _unitOfWork.TableRepository
                                                    .Get(t => request.TableIds.Contains(t.TableId) &&
                                                              t.TableType.BarId.Equals(request.BarId) && 
                                                              t.IsDeleted == false &&
                                                              t.TableType.IsDeleted == PrefixKeyConstant.FALSE,
                                                              includeProperties: "TableType");

                    if (existingTables.Count() != request.TableIds.Count)
                    {
                        throw new CustomException.InvalidDataException("Vài bàn không tồn tại, vui lòng kiểm tra lại !");
                    }
                    foreach (var tableId in request.TableIds)
                    {
                        var bookingTable = new BookingTable
                        {
                            BookingId = booking.BookingId,
                            TableId = tableId,
                        };

                        booking.BookingTables?.Add(bookingTable);
                    }
                }
                else
                {
                    throw new CustomException.InvalidDataException("Booking request does not have table field");
                }

                try
                {
                    _unitOfWork.BeginTransaction();
                    _unitOfWork.BookingRepository.Insert(booking);
                    _unitOfWork.CommitTransaction();
                    await _emailSender.SendBookingInfo(booking);

                    var bar = await _unitOfWork.BarRepository.GetByIdAsync(booking.BarId);

                    List<Guid> ids = new List<Guid>();
                    ids.Add(booking.AccountId);

                    var fcmNotification = new CreateNotificationRequest
                    {
                        BarId = booking.Bar.BarId,
                        MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                        WebDeepLink = $"booking-detail/{booking.BookingId}",
                        ImageUrl = bar == null ? null : bar.Images.Split(',')[0],
                        IsPublic = false,
                        Message = $"Đặt bàn thành công tại {bar.BarName} với mã đặt chỗ {booking.BookingCode}, quý khách hãy dùng mã đặt chỗ hoặc mã QR để thực hiện check-in khi đến quán.",
                        Title = $"Đặt bàn thành công tại {bar.BarName}!",
                        Type = FcmNotificationType.BOOKING,
                        SpecificAccountIds = ids
                    };

                    await _fcmService.CreateAndSendNotification(fcmNotification);

                    var accounts = await _unitOfWork.AccountRepository.GetAsync(a => a.BarId == booking.BarId && a.Role.RoleName == "STAFF", includeProperties: "Role");

                    if (accounts.Any())
                    {
                        List<Guid> staffIds = new List<Guid>();

                        foreach (var account in accounts)
                        {
                            staffIds.Add(account.AccountId);
                        }

                        var fcmNotificationForStaff = new CreateNotificationRequest
                        {
                            BarId = booking.Bar.BarId,
                            MobileDeepLink = null,
                            WebDeepLink = $"staff/table-registration-detail/{booking.BookingId}",
                            ImageUrl = bar == null ? null : bar.Images.Split(',')[0],
                            IsPublic = false,
                            Message = $"Đơn đăt chỗ mới với mã đặt chỗ {booking.BookingCode} đã được đặt vào ngày {booking.BookingDate.ToString("dd/MM/yyyy")} lúc {booking.BookingTime.Hours:D2}:{booking.BookingTime.Minutes:D2}.",
                            Title = $"Có đơn đặt chỗ mới tại {bar.BarName}!",
                            Type = FcmNotificationType.BOOKING,
                            SpecificAccountIds = staffIds
                        };

                        await _fcmService.CreateAndSendNotification(fcmNotificationForStaff);
                    }
                }
                catch (Exception ex)
                {
                    _unitOfWork.RollBack();
                    throw new InternalServerErrorException($"An Internal error occurred while creating customer: {ex.Message}");
                }
                finally
                {
                    _unitOfWork.Dispose();
                }
                return _mapper.Map<BookingResponse>(booking);
            }
            catch (DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (CustomException.InvalidDataException ex)
            {
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task UpdateBookingStatus(Guid BookingId, int Status)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var booking = (await _unitOfWork.BookingRepository
                                         .GetAsync(b => b.BookingId == BookingId,
                                                includeProperties: "BookingExtraDrinks,Bar"))
                                         .FirstOrDefault();
                _unitOfWork.BeginTransaction();

                if (booking == null)
                {
                    throw new DataNotFoundException("Không tìm thấy đơn Đặt bàn.");
                }

                if (!getAccount.BarId.Equals(booking?.BarId))
                {
                    throw new UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                switch (booking?.Status)
                {
                    case 1:
                        if (Status == (int)PrefixValueEnum.Serving ||
                            Status == (int)PrefixValueEnum.Completed ||
                            Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            throw new CustomException.InvalidDataException("Không thể thực hiện check-in: Lịch đặt chỗ đã bị hủy");
                        }
                        break;

                    case 0:
                        if (Status == (int)PrefixValueEnum.Serving &&
                                      (booking.BookingDate.Date != DateTime.Now.Date ||
                                       booking.BookingTime > DateTime.Now.AddMinutes(45).TimeOfDay) ||
                            Status == (int)PrefixValueEnum.Completed)
                        {
                            throw new CustomException.InvalidDataException("Không thể thực hiện check-in: vẫn chưa đến ngày hoặc thời gian đặt bàn");
                        }
                        break;

                    case 2:
                        if (Status == (int)PrefixValueEnum.PendingBooking || Status == (int)PrefixValueEnum.Cancelled)
                        {
                            throw new CustomException.InvalidDataException("Không thể chuyển từ trạng thái đang phục vụ về pending hoặc cancelled");
                        }
                        break;

                    case 3:
                        throw new CustomException.InvalidDataException("Không thể thay đổi trạng thái của booking đã hoàn thành");
                }

                if (Status == 3)
                {
                    booking.CheckOutStaffId = accountId;
                    if (booking.AdditionalFee < 0)
                    {
                        throw new CustomException.InvalidDataException("Dịch vụ cộng thêm không thể nhỏ hơn 0");
                    }
                }

                if (Status == 2 || Status == 3)
                {
                    var bookingTables = _unitOfWork.BookingTableRepository.Get(t => t.BookingId == booking.BookingId);
                    foreach (var bookingTable in bookingTables)
                    {
                        var table = _unitOfWork.TableRepository
                                                .Get(x => x.TableId.Equals(bookingTable.TableId) && x.IsDeleted == false &&
                                                          x.TableType.IsDeleted == PrefixKeyConstant.FALSE)
                                                .FirstOrDefault();
                        if (table == null)
                        {
                            throw new CustomException.DataNotFoundException("Không tìm thấy bàn");
                        }
                        if (Status == 2)
                        {
                            table.Status = 1;
                        }
                        else
                        {
                            table.Status = 0;
                        }
                        _unitOfWork.TableRepository.Update(table);
                        _unitOfWork.Save();
                    }
                }

                booking.Status = Status;
                _unitOfWork.BookingRepository.Update(booking);

                switch (booking.Status)
                {
                    case 1:
                        await SendNotiBookingSts(booking.Bar, booking,
                            string.Format(PrefixKeyConstant.BOOKING_CANCEL_NOTI, booking.Bar.BarName, booking.BookingCode,
                                          booking.BookingTime, booking.BookingDate.ToString("dd/MM/yyyy")),
                            string.Format(PrefixKeyConstant.BOOKING_CANCEL_TITLE, booking.Bar.BarName));
                        break;

                    case 2:
                        await SendNotiBookingSts(booking.Bar, booking, PrefixKeyConstant.BOOKING_SERVING_CONTENT_NOTI,
                            string.Format(PrefixKeyConstant.BOOKING_SERVING_TITLE_NOTI, booking.Bar.BarName));
                        break;

                    case 3:
                        await SendNotiBookingSts(booking.Bar, booking, PrefixKeyConstant.BOOKING_COMPLETED_CONTENT_NOTI,
                            string.Format(PrefixKeyConstant.BOOKING_COMPLETED_TITLE_NOTI, booking.Bar.BarName));
                        break;
                }

                _unitOfWork.Save();
                _unitOfWork.CommitTransaction();
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                _unitOfWork.RollBack();
                throw new CustomException.UnAuthorizedException(ex.Message);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                _unitOfWork.RollBack();
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (CustomException.InvalidDataException ex)
            {
                _unitOfWork.RollBack();
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<PaymentLink> CreateBookingTableWithDrinks(BookingDrinkRequest request, HttpContext httpContext, bool isMobile = false)
        {
            try
            {
                var booking = _mapper.Map<Booking>(request);
                if (request?.TableIds?.Count() > 6)
                {
                    throw new CustomException.InvalidDataException("Chỉ được đặt tối đa 5 bàn");
                }
                await ValidateTableAvailability(request.TableIds, request.BookingDate, request.BookingTime, request.BarId);

                booking.Account = _unitOfWork.AccountRepository.GetByID(_authentication.GetUserIdFromHttpContext(httpContext))
                    ?? throw new DataNotFoundException("Không tìm thấy tài khoản !");
                booking.Bar = _unitOfWork.BarRepository.GetByID(request.BarId)
                    ?? throw new DataNotFoundException("Không tìm thấy quán bar !");

                var barTimes = await _unitOfWork.BarTimeRepository.GetAsync(x => x.BarId == request.BarId);
                if (barTimes == null || !barTimes.Any())
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy thông tin thời gian của Bar.");
                }

                Utils.ValidateOpenCloseTimeWithTimeSlot(request.BookingDate, request.BookingTime,
                    barTimes.ToList(), booking.Bar.TimeSlot);

                booking.BookingTables = booking.BookingTables ?? new List<BookingTable>();
                booking.BookingDrinks = booking.BookingDrinks ?? new List<BookingDrink>();
                booking.BookingDate = request.BookingDate.Date;
                booking.BookingCode = $"{booking.BookingDate.ToString("yyMMdd")}{RandomHelper.GenerateRandomNumberString()}";
                booking.Status = (int)PrefixValueEnum.PendingBooking;
                booking.BookingTime = request.BookingTime;
                booking.CreateAt = TimeHelper.ConvertDateTimeToUtcPlus7(DateTime.UtcNow);
                booking.NumOfTable = request?.TableIds?.Count ?? 0;
                var qrCode = _qrCodeService.GenerateQRCode(booking.BookingId);
                booking.QRTicket = await _firebase.UploadImageAsync(Utils.ConvertBase64ToFile(qrCode));

                booking.ExpireAt = (request.BookingDate + request.BookingTime).AddHours(2);

                double totalPrice = 0;

                if (request.TableIds != null && request.TableIds.Count > 0)
                {
                    var existingTables = _unitOfWork.TableRepository
                                                        .Get(t => request.TableIds.Contains(t.TableId) &&
                                                                  t.TableType.BarId.Equals(request.BarId) &&
                                                                  t.IsDeleted == false &&
                                                          t.TableType.IsDeleted == PrefixKeyConstant.FALSE,
                                                        includeProperties: "TableType");
                    if (existingTables.Count() != request.TableIds.Count)
                    {
                        throw new CustomException.InvalidDataException("Some TableIds do not exist.");
                    }
                    foreach (var tableId in request.TableIds)
                    {
                        var bookingTable = new BookingTable
                        {
                            BookingId = booking.BookingId,
                            TableId = tableId,
                        };

                        booking.BookingTables?.Add(bookingTable);
                    }
                }
                else
                {
                    throw new CustomException.InvalidDataException("Booking request does not have table field");
                }

                if (request.Drinks != null && request.Drinks.Count > 0)
                {
                    var drinkIds = request.Drinks.Select(drink => drink.DrinkId).ToList();
                    var existingDrinks = _unitOfWork.DrinkRepository
                                                    .Get(d => drinkIds.Contains(d.DrinkId) &&
                                                              d.BarId.Equals(request.BarId),
                                                        includeProperties: "DrinkCategory");
                    double discoutVoucher = 0;
                    double maxPriceVoucher = 0;
                    double discountMount = 0;
                    if (existingDrinks.Count() != request.Drinks.Count)
                    {
                        throw new CustomException.InvalidDataException("Some DrinkIds do not exist.");
                    }
                    foreach (var drink in request.Drinks)
                    {
                        var bookingDrink = new BookingDrink
                        {
                            BookingId = booking.BookingId,
                            DrinkId = drink.DrinkId,
                            ActualPrice = existingDrinks.FirstOrDefault(e => e.DrinkId == drink.DrinkId &&
                                                              e.BarId.Equals(request.BarId)).Price,
                            Quantity = drink.Quantity

                        };
                        totalPrice += bookingDrink.ActualPrice * bookingDrink.Quantity;
                        booking.TotalPrice = totalPrice;
                        booking.BookingDrinks?.Add(bookingDrink);
                    }
                    totalPrice = totalPrice - totalPrice * booking.Bar.Discount / 100;

                    if (request.VoucherCode != null)
                    {

                        var voucherQuery = new VoucherQueryRequest
                        {
                            barId = request.BarId,
                            bookingDate = booking.BookingDate,
                            bookingTime = booking.BookingTime,
                            voucherCode = request.VoucherCode
                        };

                        var voucher = await _eventVoucherService
                                                .GetVoucherByCode(voucherQuery);

                        discoutVoucher = voucher.Discount;
                        maxPriceVoucher = voucher.MaxPrice;

                        if (voucher?.Quantity != null)
                        {
                            voucher.Quantity -= 1;
                            if (voucher.Quantity == 0)
                            {
                                voucher.Status = PrefixKeyConstant.FALSE;
                            }
                            await _eventVoucherService.UpdateStatusNStsVoucher(voucher.EventVoucherId, voucher.Quantity, voucher.Status);
                        }

                        if (voucher?.Discount > 0)
                        {
                            discountMount = totalPrice * (discoutVoucher / 100);
                            if (discountMount > voucher?.MaxPrice)
                            {
                                discountMount = voucher.MaxPrice;
                            }
                            totalPrice -= discountMount;
                        }
                    }
                }


                try
                {
                    booking.TotalPrice = totalPrice;
                    _unitOfWork.BeginTransaction();
                    _unitOfWork.BookingRepository.Insert(booking);
                    _unitOfWork.CommitTransaction();
                    await _emailSender.SendBookingInfo(booking, totalPrice);

                    return _paymentService.GetPaymentLink(booking.BookingId, booking.AccountId,
                                request.PaymentDestination, totalPrice, isMobile);
                }
                catch (Exception ex)
                {
                    _unitOfWork.RollBack();
                    throw new InternalServerErrorException($"An Internal error occurred: {ex.Message}");
                }
            }
            catch (DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (CustomException.InvalidDataException ex)
            {
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        private async Task ValidateTableAvailability(List<Guid> tableIds, DateTime bookingDate, 
            TimeSpan bookingTime, Guid barId)
        {
            try
            {
                var bookingsInDate = await _unitOfWork.BookingRepository
                    .GetAsync(b => b.BookingDate.Date == bookingDate.Date && 
                        b.BookingTime == bookingTime &&
                        b.BarId == barId &&
                        (b.Status == (int)PrefixValueEnum.PendingBooking ||
                        b.Status == (int)PrefixValueEnum.Serving),
                     includeProperties: "BookingTables");

                foreach (var tableId in tableIds)
                {
                    var conflictBookings = bookingsInDate.Where(b =>
                        b.BookingTables.Any(bt => bt.TableId == tableId));

                    if (conflictBookings.Any()) 
                    {
                        throw new CustomException.InvalidDataException($"Bàn đã được đặt trong ngày {bookingDate.ToString(@"dd/mm/yyyy")} " +
                            $"tại thời gian {bookingTime.ToString(@"hh\:mm")} ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<BookingCustomResponse>> GetAllBookingByStsPending()
        {
            try
            {
                DateTimeOffset now = DateTimeOffset.Now;
                TimeSpan roundedTimeOfDay = TimeSpan.FromHours(now.TimeOfDay.Hours)
                                                   .Add(TimeSpan.FromMinutes(now.TimeOfDay.Minutes))
                                                   .Add(TimeSpan.FromSeconds(now.TimeOfDay.Seconds));
                TimeSpan roundedTwoHoursLater = new TimeSpan(
                                                    now.AddHours(2).TimeOfDay.Hours,
                                                    now.AddHours(2).TimeOfDay.Minutes,
                                                    now.AddHours(2).TimeOfDay.Seconds
                                                    );
                var getBkByStsPending = await _unitOfWork.BookingRepository
                    .GetAsync(filter: x => (
                        (x.BookingDate.Date == now.Date &&
                         x.BookingTime >= roundedTimeOfDay &&
                         x.BookingTime <= roundedTwoHoursLater)
                        ||
                        (x.BookingDate.Date == now.Date.AddDays(1) &&
                         x.BookingTime <= roundedTwoHoursLater)
                    ) &&
                    x.Status == (int)PrefixValueEnum.PendingBooking,
                    includeProperties: "Bar");

                if (getBkByStsPending.IsNullOrEmpty())
                {
                    return new List<BookingCustomResponse>();
                }

                var response = _mapper.Map<List<BookingCustomResponse>>(getBkByStsPending);
                return response;
            }
            catch (InternalServerErrorException ex)
            {
                throw new InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task<List<BookingCustomResponse>> GetAllBookingByStsPendingCus()
        {
            try
            {
                DateTimeOffset now = DateTimeOffset.Now;
                TimeSpan roundedTimeOfDay = TimeSpan.FromHours(now.TimeOfDay.Hours)
                                                   .Add(TimeSpan.FromMinutes(now.TimeOfDay.Minutes))
                                                   .Add(TimeSpan.FromSeconds(now.TimeOfDay.Seconds));
                TimeSpan roundedTwoHoursLater = new TimeSpan(
                                                    now.AddHours(2).TimeOfDay.Hours,
                                                    now.AddHours(2).TimeOfDay.Minutes,
                                                    now.AddHours(2).TimeOfDay.Seconds
                                                    );
                var getBkByStsPending = await _unitOfWork.BookingRepository
                    .GetAsync(filter: x => x.BookingDate.Date == now.Date &&
                                            x.BookingTime <= roundedTimeOfDay &&
                                            x.Status == (int)PrefixValueEnum.PendingBooking,
                                            includeProperties: "Bar");

                if (getBkByStsPending.IsNullOrEmpty())
                {
                    return new List<BookingCustomResponse>();
                }

                var response = _mapper.Map<List<BookingCustomResponse>>(getBkByStsPending);
                return response;
            }
            catch (InternalServerErrorException ex)
            {
                throw new InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task UpdateStsBookingServing(Guid bookingId)
        {
            try
            {
                var timeNow = DateTimeOffset.Now.AddMinutes(45).TimeOfDay;
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);
                var getBooking = _unitOfWork.BookingRepository.GetByID(bookingId);

                if (!getAccount.BarId.Equals(getBooking.BarId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (getBooking.BookingDate.Date != DateTime.Now.Date)
                {
                    throw new CustomException.InvalidDataException("Đơn booking này chưa tới ngày check-in !");
                }

                if (getBooking.BookingTime > DateTimeOffset.Now.AddMinutes(45).TimeOfDay)
                {
                    throw new CustomException.InvalidDataException("Chưa tới giờ check-in !");
                }

                if (getBooking.Status != (int)PrefixValueEnum.PendingBooking)
                {
                    throw new CustomException.InvalidDataException("Đơn này phải ở trạng thái đang chờ !");
                }

                getBooking.CheckInStaffId = accountId;
                getBooking.Status = (int)PrefixValueEnum.Serving;
                await _unitOfWork.BookingRepository.UpdateRangeAsync(getBooking);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task<(List<StaffBookingReponse> responses, int TotalPage)> GetListBookingAuthorizedAdmin(string qrTicket, Guid? BarId, string? CustomerName, string? Phone, string? Email, DateTimeOffset? bookingDate, TimeSpan? bookingTime, int? Status, int PageIndex, int PageSize)
        {
            try
            {
                var responses = new List<StaffBookingReponse>();


                if (BarId != Guid.Empty)
                {
                    var Bar = await _unitOfWork.BarRepository.GetByIdAsync(BarId);
                    if (Bar == null)
                    {
                        throw new CustomException.DataNotFoundException("Không có thông tin của Bar");
                    }
                }

                Expression<Func<Booking, bool>> filter = b =>
                (BarId == Guid.Empty || BarId.HasValue && b.BarId == BarId) &&
                (string.IsNullOrEmpty(CustomerName) || b.Account.Fullname.Contains(CustomerName)) &&
                (string.IsNullOrEmpty(Phone) || b.Account.Phone.Contains(Phone)) &&
                (string.IsNullOrEmpty(Email) || b.Account.Email.Contains(Email)) &&
                (!bookingDate.HasValue || b.BookingDate.Date.Day == bookingDate.Value.Date.Day) &&
                (!bookingTime.HasValue || b.BookingTime == bookingTime.Value) &&
                (!Status.HasValue || b.Status == Status.Value) &&
                (string.IsNullOrEmpty(qrTicket) || b.BookingId.Equals(Guid.Parse(qrTicket)));

                var bookings = await _unitOfWork.BookingRepository.GetAsync(filter);

                int totalPage = 1;
                if (bookings.Count() > PageSize)
                {
                    if (PageSize == 1)
                    {
                        totalPage = (bookings.Count() / PageSize);
                    }
                    else
                    {
                        totalPage = (bookings.Count() / PageSize) + 1;
                    }
                }

                var bookingsWithPagination = await _unitOfWork.BookingRepository.GetAsync(filter: filter, includeProperties: "Account,Bar", pageIndex: PageIndex, pageSize: PageSize, orderBy: o => o.OrderByDescending(b => b.BookingDate).ThenByDescending(b => b.BookingTime));

                foreach (var booking in bookingsWithPagination)
                {
                    var response = new StaffBookingReponse
                    {
                        BookingDate = booking.BookingDate,
                        BookingId = booking.BookingId,
                        BookingTime = booking.BookingTime,
                        CustomerName = booking.Account.Fullname,
                        Email = booking.Account.Email,
                        Phone = booking.Account.Phone,
                        Status = booking.Status,
                        BookingCode = booking.BookingCode,
                        AdditionalFee = booking.AdditionalFee,
                        TotalPrice = booking.TotalPrice,
                        QRTicket = booking.QRTicket,
                        Note = booking.Note,
                    };
                    responses.Add(response);
                }
                return (responses, totalPage);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<BookingDetailByStaff> GetBookingDetailAuthorizedAdmin(Guid BookingId)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var response = new BookingDetailByStaff();

                var booking = (await _unitOfWork.BookingRepository
                                                .GetAsync(b => b.BookingId == BookingId,
                                                          includeProperties: "Account,Bar,BookingExtraDrinks.Drink,BookingDrinks.Drink"))
                                                .FirstOrDefault();

                if (booking == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy Id Đặt bàn");
                }


                response.BookingId = booking.BookingId;
                response.BookingTime = booking.BookingTime;
                response.Status = booking.Status;
                response.CreateAt = booking.CreateAt;
                response.BookingDate = booking.BookingDate;
                response.BarName = booking.Bar.BarName;
                response.BarAddress = booking.Bar.Address;
                response.CustomerPhone = booking.Account.Phone;
                response.CustomerName = booking.Account.Fullname;
                response.CustomerEmail = booking.Account.Email;
                response.BookingCode = booking.BookingCode;
                response.AdditionalFee = booking.AdditionalFee;
                response.TotalPrice = booking.TotalPrice;
                response.Note = booking.Note;
                response.QRTicket = booking.QRTicket;
                response.NumOfPeople = booking.NumOfPeople;
                response.NumOfTable = booking.NumOfTable;

                if (booking.TotalPrice >= 0 || (booking.AdditionalFee >= 0 && booking.AdditionalFee.HasValue))
                {
                    response.bookingDrinksList = _mapper.Map<List<BookingDrinkDetailResponse>>(booking.BookingDrinks);
                    response.BookingDrinkExtraResponses = _mapper.Map<List<BookingDrinkExtraResponse>>(booking.BookingExtraDrinks);
                }

                var bookingTables = await _unitOfWork.BookingTableRepository.GetAsync(bt => bt.BookingId == BookingId);
                foreach (var table in bookingTables)
                {
                    var Tables = (await _unitOfWork.TableRepository.GetAsync(t => t.TableId == table.TableId, includeProperties: "TableType")).FirstOrDefault();
                    var BookingTable = new BookingTableResponseByStaff
                    {
                        TableName = Tables.TableName,
                        TableTypeName = Tables.TableType.TypeName
                    };
                    response.bookingTableList.Add(BookingTable);
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<BookingDrinkDetailResponse>> ExtraDrinkInServing(Guid bookingId, List<DrinkRequest> request)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                            .Get(filter: x => x.AccountId.Equals(accountId),
                                                 includeProperties: "Role")
                                            .FirstOrDefault();
                var getBooking = _unitOfWork.BookingRepository
                                            .Get(filter: x => x.BookingId.Equals(bookingId),
                                                 includeProperties: "Bar")
                                            .FirstOrDefault();
                if (!getAccount.BarId.HasValue && getAccount.Role.RoleName.Equals("CUSTOMER"))
                {
                    if (!getAccount.AccountId.Equals(getBooking.AccountId))
                    {
                        throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào đơn booking này !");
                    }
                }
                if (getAccount.BarId.HasValue && !getAccount.BarId.Equals(getBooking.BarId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }
                if (getBooking.Status != (int)PrefixValueEnum.Serving)
                {
                    throw new CustomException.InvalidDataException("Không thể thêm đồ uống khi đơn không ở trạng thái đang phục vụ !");
                }

                var getAccStaffOfBar = _unitOfWork.AccountRepository
                             .Get(filter: x => x.BarId.Equals(getBooking.BarId) &&
                                       x.Status == 1 &&
                                       x.Role.RoleName.Equals(PrefixKeyConstant.STAFF),
                                  includeProperties: "Role")
                             .Select(x => x.AccountId)
                             .ToList();
                var getTimeSlot = getBooking?.Bar.TimeSlot ?? 0;

                if (getBooking.BookingDate.Date != DateTime.Now.Date ||
                    (getBooking.BookingDate.Date == DateTime.Now.Date &&
                     getBooking.BookingTime >= DateTime.Now.TimeOfDay &&
                     getBooking.BookingTime.Add(TimeSpan.FromHours(getTimeSlot)) <= DateTime.Now.TimeOfDay))
                {
                    throw new CustomException.InvalidDataException("Thời gian đặt đồ uống phải nằm trong thời gian phục vụ !");
                }

                var extraDrink = new List<BookingExtraDrink>();
                double addFeeExtraDrink = 0;
                _unitOfWork.BeginTransaction();

                foreach (var drink in request)
                {
                    var isDrink = _unitOfWork.DrinkRepository
                                             .Get(x => x.DrinkId.Equals(drink.DrinkId) &&
                                                       x.BarId.Equals(getBooking.BarId) &&
                                                       x.Status == PrefixKeyConstant.TRUE)
                                             .FirstOrDefault();
                    if (isDrink == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy đồ uống !");
                    }

                    if (getBooking.AdditionalFee == null)
                    {
                        getBooking.AdditionalFee = 0;
                    }

                    var isExistDrink = _unitOfWork.BookingExtraDrinkRepository
                                                    .Get(filter: x => x.DrinkId.Equals(drink.DrinkId) &&
                                                                      x.BookingId.Equals(bookingId) &&
                                                                      x.Booking.Bar.BarId.Equals(getBooking.BarId) &&
                                                                      (x.Status == (int)ExtraDrinkStsEnum.Pending ||
                                                                      x.Status == (int)ExtraDrinkStsEnum.Preparing),
                                                         includeProperties: "Drink,Booking.Bar")
                                                    .FirstOrDefault();

                    var mapper = _mapper.Map<BookingExtraDrink>(drink);

                    if (isExistDrink != null)
                    {
                        _ = getAccount.Role.RoleName.Equals(PrefixKeyConstant.CUSTOMER) ?
                                                    isExistDrink.CustomerId = accountId :
                                                    isExistDrink.StaffId = accountId;
                        isExistDrink.Quantity += drink.Quantity;
                        await _unitOfWork.BookingExtraDrinkRepository.UpdateRangeAsync(isExistDrink);
                    }
                    else
                    {
                        mapper.BookingId = getBooking.BookingId;
                        mapper.ActualPrice = isDrink.Price;
                        mapper.CreatedDate = DateTime.Now;
                        mapper.UpdatedDate = mapper.CreatedDate;
                        _ = getAccount.Role.RoleName.Equals(PrefixKeyConstant.CUSTOMER) ?
                                                    mapper.CustomerId = accountId :
                                                    mapper.StaffId = accountId;
                        _ = getAccount.Role.RoleName.Equals(PrefixKeyConstant.CUSTOMER) ?
                                                    mapper.Status = (int)ExtraDrinkStsEnum.Pending :
                                                    mapper.Status = (int)ExtraDrinkStsEnum.Preparing;
                        await _unitOfWork.BookingExtraDrinkRepository.InsertAsync(mapper);
                    }
                    await Task.Delay(10);
                    await _unitOfWork.SaveAsync();
                    extraDrink.Add(mapper);
                    addFeeExtraDrink += isDrink.Price * drink.Quantity;
                }
                getBooking.AdditionalFee += addFeeExtraDrink;
                await _unitOfWork.BookingRepository.UpdateRangeAsync(getBooking);
                await Task.Delay(100);

                var fcmNotification = new CreateNotificationRequest
                {
                    BarId = getBooking.Bar.BarId,
                    MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{getBooking.BookingId}",
                    WebDeepLink = $"staff/booking-detail/{getBooking.BookingId}",
                    ImageUrl = getBooking.Bar == null ? null : getBooking.Bar.Images.Split(',')[0],
                    IsPublic = false,
                    Message = string.Format(PrefixKeyConstant.EXTRA_DRINK_CONTENT, getBooking.BookingCode),
                    Title = PrefixKeyConstant.EXTRA_DRINK_TITLE_NOTI,
                    Type = FcmNotificationType.BOOKING,
                    SpecificAccountIds = getAccStaffOfBar
                };

                var getAllExtraDrinkOfBk = _unitOfWork.BookingExtraDrinkRepository
                                                      .Get(filter: x => x.BookingId.Equals(bookingId) && 
                                                                x.Status != (int)ExtraDrinkStsEnum.Preparing,
                                                            includeProperties: "Drink");
                await _fcmService.CreateAndSendNotification(fcmNotification);
                await _unitOfWork.SaveAsync();
                _unitOfWork.CommitTransaction();
                var response = _mapper.Map<List<BookingDrinkDetailResponse>>(getAllExtraDrinkOfBk);
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                _unitOfWork.Dispose();
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task<List<BookingDrinkDetailResponse>> UpdExtraDrinkInServing(Guid bookingId, List<UpdBkDrinkExtraRequest> request)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                            .Get(filter: x => x.AccountId.Equals(accountId),
                                                 includeProperties: "Role")
                                            .FirstOrDefault();
                var getBooking = _unitOfWork.BookingRepository
                                            .Get(filter: x => x.BookingId.Equals(bookingId),
                                                 includeProperties: "Bar")
                                            .FirstOrDefault();

                if (!getAccount.BarId.HasValue && getAccount.Role.RoleName.Equals("CUSTOMER"))
                {
                    if (!getAccount.AccountId.Equals(getBooking.AccountId))
                    {
                        throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào đơn booking này !");
                    }
                }
                if (getAccount.BarId.HasValue && !getAccount.BarId.Equals(getBooking.BarId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }
                if (getBooking.Status != (int)PrefixValueEnum.Serving)
                {
                    throw new CustomException.InvalidDataException("Không thể thêm đồ uống khi đơn không ở trạng thái đang phục vụ !");
                }

                var getTimeSlot = getBooking?.Bar.TimeSlot ?? 0;

                if (getBooking.BookingDate.Date != DateTime.Now.Date ||
                    (getBooking.BookingDate.Date == DateTime.Now.Date &&
                     getBooking.BookingTime >= DateTime.Now.TimeOfDay &&
                     getBooking.BookingTime.Add(TimeSpan.FromHours(getTimeSlot)) <= DateTime.Now.TimeOfDay))
                {
                    throw new CustomException.InvalidDataException("Thời gian đặt đồ uống phải nằm trong thời gian phục vụ !");
                }

                var listExtraDrink = _unitOfWork.BookingExtraDrinkRepository
                                                .Get(filter: x => x.BookingId.Equals(getBooking.BookingId) && x.Status != (int)ExtraDrinkStsEnum.Delivered);
                var delExtraDrink = listExtraDrink.Where(ex => !request.Any(x => x.DrinkId.Equals(ex.DrinkId))).Distinct().ToList();
                var newExtraDrink = request.Where(ex => !listExtraDrink.Any(x => x.DrinkId.Equals(ex.DrinkId))).Distinct().ToList();
                var updExtraDrink = request.Where(ex => listExtraDrink/*.Where(x => x.Status != (int)ExtraDrinkStsEnum.Delivered)
                                                                      */.Any(x => x.DrinkId.Equals(ex.DrinkId) && x.Quantity != ex.Quantity))
                                                                      .Distinct().ToList();
                var extraDrink = new List<BookingExtraDrink>();

                _unitOfWork.BeginTransaction();
                foreach (var newDrink in newExtraDrink)
                {
                    var isDrink = _unitOfWork.DrinkRepository
                                             .Get(x => x.DrinkId.Equals(newDrink.DrinkId) &&
                                                       x.BarId.Equals(getBooking.BarId) &&
                                                       x.Status == PrefixKeyConstant.TRUE)
                                             .FirstOrDefault();
                    if (isDrink == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy đồ uống !");
                    }

                    var isExistDrink = _unitOfWork.BookingExtraDrinkRepository
                                                    .Get(filter: x => x.DrinkId.Equals(newDrink.DrinkId) &&
                                                                      x.BookingId.Equals(bookingId) &&
                                                                      x.Booking.Bar.BarId.Equals(getBooking.BarId) &&
                                                                      x.Status != (int)ExtraDrinkStsEnum.Delivered,
                                                         includeProperties: "Drink,Booking.Bar")
                                                    .FirstOrDefault();

                    var mapper = _mapper.Map<BookingExtraDrink>(newDrink);

                    if (isExistDrink != null)
                    {
                        isExistDrink.StaffId = accountId;
                        isExistDrink.Quantity += newDrink.Quantity;
                        await _unitOfWork.BookingExtraDrinkRepository.UpdateRangeAsync(mapper);
                    }
                    else
                    {
                        mapper.BookingId = getBooking.BookingId;
                        mapper.ActualPrice = isDrink.Price;
                        mapper.StaffId = accountId;
                        mapper.CreatedDate = DateTime.Now;
                        mapper.UpdatedDate = mapper.CreatedDate;
                        mapper.Status = (int)ExtraDrinkStsEnum.Preparing;
                        await _unitOfWork.BookingExtraDrinkRepository.InsertAsync(mapper);
                    }
                    await Task.Delay(10);
                    await _unitOfWork.SaveAsync();
                    extraDrink.Add(mapper);
                    getBooking.AdditionalFee += isDrink.Price * newDrink.Quantity;
                }
                foreach (var updDrink in updExtraDrink)
                {
                    var isDrink = _unitOfWork.DrinkRepository
                         .Get(x => x.DrinkId.Equals(updDrink.DrinkId) &&
                                   x.BarId.Equals(getBooking.BarId) &&
                                   x.Status == PrefixKeyConstant.TRUE)
                         .FirstOrDefault();
                    if (isDrink == null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy đồ uống !");
                    }

                    var isExistDrink = _unitOfWork.BookingExtraDrinkRepository
                                                    .Get(filter: x => x.DrinkId.Equals(updDrink.DrinkId) &&
                                                                      x.BookingId.Equals(bookingId) &&
                                                                      x.Booking.Bar.BarId.Equals(getBooking.BarId) &&
                                                                      x.Status != (int)ExtraDrinkStsEnum.Delivered,
                                                         includeProperties: "Drink,Booking.Bar")
                                                    .FirstOrDefault();

                    var quantityDifference = updDrink.Quantity - isExistDrink.Quantity;
                    getBooking.AdditionalFee += isExistDrink.Drink.Price * quantityDifference;

                    isExistDrink.StaffId = accountId;
                    isExistDrink.UpdatedDate = DateTime.Now;
                    isExistDrink.Quantity = updDrink.Quantity;
                    isExistDrink.StaffId = accountId;
                    await _unitOfWork.BookingExtraDrinkRepository.UpdateRangeAsync(isExistDrink);

                    await Task.Delay(100);
                    await _unitOfWork.SaveAsync();
                    extraDrink.Add(isExistDrink);
                }
                foreach (var delDrink in delExtraDrink)
                {
                    var isExistDrink = _unitOfWork.BookingExtraDrinkRepository
                                                    .Get(filter: x => x.DrinkId.Equals(delDrink.DrinkId) &&
                                                                      x.BookingExtraDrinkId.Equals(delDrink.BookingExtraDrinkId) &&
                                                                      x.Booking.BarId.Equals(getAccount.BarId))
                                                    .FirstOrDefault();
                    if (isExistDrink is null)
                    {
                        throw new CustomException.DataNotFoundException("Không tìm thấy đồ uống ở quán bar này !");
                    }
                    getBooking.AdditionalFee -= isExistDrink.ActualPrice * isExistDrink.Quantity;
                    await _unitOfWork.BookingExtraDrinkRepository.DeleteAsync(isExistDrink.BookingExtraDrinkId);
                    await Task.Delay(10);
                    await _unitOfWork.SaveAsync();
                }

                await _unitOfWork.BookingRepository.UpdateRangeAsync(getBooking);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
                var getAllExtraDrinkOfBk = _unitOfWork.BookingExtraDrinkRepository
                                                      .Get(filter: x => x.BookingId.Equals(bookingId) &&
                                                                x.Status != (int)ExtraDrinkStsEnum.Preparing,
                                                            includeProperties: "Drink");
                var response = _mapper.Map<List<BookingDrinkDetailResponse>>(getAllExtraDrinkOfBk);
                
                _unitOfWork.CommitTransaction();
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                _unitOfWork.Dispose();
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task<List<BookingDrinkDetailResponse>> GetExtraBookingServing(Guid bookingId)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                            .Get(filter: x => x.AccountId.Equals(accountId),
                                                 includeProperties: "Role")
                                            .FirstOrDefault();
                var getBooking = await _unitOfWork.BookingExtraDrinkRepository
                                            .GetAsync(filter: x => x.BookingId.Equals(bookingId),
                                                includeProperties: "Booking,Drink");

                if (!getAccount.BarId.HasValue && getAccount.Role.RoleName.Equals("CUSTOMER"))
                {
                    if (!getBooking.Any(x => x.Booking.AccountId.Equals(getAccount.AccountId)) && getBooking != null)
                    {
                        throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào đơn booking này !");
                    }

                }
                if (getAccount.BarId.HasValue && !getBooking.Any(x => x.Booking.BarId.Equals(getAccount.BarId)))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                var response = _mapper.Map<List<BookingDrinkDetailResponse>>(getBooking);
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
        public async Task UpdateBookingStatusJob(Guid accountId, Guid BookingId, int Status)
        {
            try
            {
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var booking = (await _unitOfWork.BookingRepository
                                         .GetAsync(b => b.BookingId == BookingId,
                                                includeProperties: "BookingExtraDrinks"))
                                         .FirstOrDefault();
                _unitOfWork.BeginTransaction();

                if (booking == null)
                {
                    throw new DataNotFoundException("Không tìm thấy đơn Đặt bàn.");
                }

                if (booking.Status == 1 && (Status == 2 || Status == 3))
                {
                    throw new CustomException.InvalidDataException("Không thể thực hiện check-in: Lịch đặt chỗ đã bị hủy");
                }
                if (booking.Status == 0 && booking.BookingDate.Date != DateTime.Now.Date && (Status == 2 || Status == 3))
                {
                    throw new CustomException.InvalidDataException("Không thể thực hiện check-in: vẫn chưa đến ngày đặt bàn");
                }
                if (Status == 3 && booking.BookingDrinks != null)
                {
                    booking.AdditionalFee = booking.BookingExtraDrinks
                        .Sum(x => x.ActualPrice * x.Quantity);
                }
                else
                {
                    booking.AdditionalFee = null;
                }

                switch (booking.Status)
                {
                    case 1:
                        if (Status == (int)PrefixValueEnum.Serving ||
                            Status == (int)PrefixValueEnum.Completed ||
                            Status == (int)PrefixValueEnum.PendingBooking)
                        {
                            throw new CustomException.InvalidDataException("Không thể thực hiện check-in: Lịch đặt chỗ đã bị hủy");
                        }
                        break;

                    case 0:
                        if ((Status == (int)PrefixValueEnum.Serving &&
                                      (booking.BookingDate.Date != DateTime.Now.Date ||
                                       booking.BookingTime > DateTime.Now.AddMinutes(45).TimeOfDay)))
                        {
                            throw new CustomException.InvalidDataException("Không thể thực hiện check-in: vẫn chưa đến ngày hoặc thời gian đặt bàn");
                        }
                        break;

                    case 2:
                        if (Status == (int)PrefixValueEnum.PendingBooking || Status == (int)PrefixValueEnum.Cancelled)
                        {
                            throw new CustomException.InvalidDataException("Không thể chuyển từ trạng thái đang phục vụ về pending hoặc cancelled");
                        }
                        break;

                    case 3:
                        throw new CustomException.InvalidDataException("Không thể thay đổi trạng thái của booking đã hoàn thành");
                }

                if (Status == 3)
                {
                    booking.CheckOutStaffId = accountId;
                    if (booking.AdditionalFee < 0)
                    {
                        throw new CustomException.InvalidDataException("Dịch vụ cộng thêm không thể nhỏ hơn 0");
                    }
                }
                booking.Status = Status;
                _unitOfWork.BookingRepository.Update(booking);
                _unitOfWork.Save();

                if (Status == 2 || Status == 3)
                {
                    var bookingTables = _unitOfWork.BookingTableRepository.Get(t => t.BookingId == booking.BookingId);
                    foreach (var bookingTable in bookingTables)
                    {
                        var table = _unitOfWork.TableRepository
                                                .Get(x => x.TableId.Equals(bookingTable.TableId) && x.IsDeleted == false)
                                                .FirstOrDefault();
                        if (table == null)
                        {
                            throw new CustomException.DataNotFoundException("Không tìm thấy bàn");
                        }
                        if (Status == 2)
                        {
                            table.Status = 1;
                        }
                        else
                        {
                            table.Status = 0;
                        }
                        _unitOfWork.TableRepository.Update(table);
                        _unitOfWork.Save();
                    }
                }
                _unitOfWork.CommitTransaction();
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                _unitOfWork.RollBack();
                throw new CustomException.UnAuthorizedException(ex.Message);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                _unitOfWork.RollBack();
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (CustomException.InvalidDataException ex)
            {
                _unitOfWork.RollBack();
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task SendNotiBookingSts(Bar bar, Booking booking, string message, string title)
        {
            var fcmNotification = new CreateNotificationRequest
            {
                BarId = booking.Bar.BarId,
                MobileDeepLink = $"com.fptu.barbuddy://booking-detail/{booking.BookingId}",
                WebDeepLink = $"booking-detail/{booking.BookingId}",
                ImageUrl = bar == null ? null : bar.Images.Split(',')[0],
                IsPublic = false,
                Message = message,
                Title = title,
                Type = FcmNotificationType.BOOKING,
                SpecificAccountIds = new List<Guid> { booking.AccountId }
            };

            await _fcmService.CreateAndSendNotification(fcmNotification);
        }

        public async Task<List<BookingDrinkDetailResponse>> UpdateStsExtra(UpdateStsBookingExtraDrink request)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var isExistExtra = _unitOfWork.BookingExtraDrinkRepository
                                              .Get(filter: x => x.BookingExtraDrinkId.Equals(request.BookingExtraDrinkId) &&
                                                                x.BookingId.Equals(request.BookingId) &&
                                                                x.DrinkId.Equals(request.DrinkId) &&
                                                                x.Status != (int) ExtraDrinkStsEnum.Delivered,
                                                   includeProperties: "Booking")
                                              .FirstOrDefault();

                if (isExistExtra != null && !isExistExtra.Booking.BarId.Equals(getAccount.BarId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập !");
                }

                if (isExistExtra is null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy đồ uống đặt thêm cho đơn này !");
                }

                if(isExistExtra.Status == (int)ExtraDrinkStsEnum.Delivered)
                {
                    throw new CustomException.InvalidDataException("Đồ uống này đã giao thành công trước đó !");
                }

                isExistExtra.Status = (int)ExtraDrinkStsEnum.Delivered;
                isExistExtra.UpdatedDate = DateTime.Now;

                await _unitOfWork.BookingExtraDrinkRepository.UpdateRangeAsync(isExistExtra);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();

                var getAllExtraDrinkOfBk = _unitOfWork.BookingExtraDrinkRepository
                                                      .Get(filter: x => x.BookingId.Equals(request.BookingId),
                                                            includeProperties: "Drink");

                var response = _mapper.Map<List<BookingDrinkDetailResponse>>(getAllExtraDrinkOfBk);
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
    }
}
