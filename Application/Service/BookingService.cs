using Application.Common;
using Application.DTOs.Booking;
using Application.DTOs.BookingDrink;
using Application.DTOs.BookingTable;
using Application.DTOs.Events.EventVoucher;
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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
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
        private readonly INotificationService _notificationService;
        private readonly IQRCodeService _qrCodeService;
        private readonly IFirebase _firebase;
        private readonly ILogger<BookingService> _logger;
        private readonly IEventVoucherService _eventVoucherService;
        private readonly IHttpContextAccessor _contextAccessor;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IAuthentication authentication,
            IPaymentService paymentService, IEmailSender emailSender,
            INotificationService notificationService, IQRCodeService qrCodeService, IFirebase firebase,
            IEventVoucherService eventVoucherService, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authentication = authentication;
            _paymentService = paymentService;
            _emailSender = emailSender;
            _notificationService = notificationService;
            _qrCodeService = qrCodeService;
            _firebase = firebase;
            _eventVoucherService = eventVoucherService;
            _contextAccessor = contextAccessor;
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

                var creNoti = new NotificationRequest
                {
                    BarId = booking.BarId,
                    Title = booking.Bar.BarName,
                    Message = string.Format(PrefixKeyConstant.BOOKING_CANCEL_NOTI, booking.Bar.BarName, booking.BookingDate.ToString("yyyy/mm/dd"), booking.BookingTime)
                };

                // Cancelled status (temp)
                booking.Status = 1;
                _unitOfWork.BeginTransaction();
                await _unitOfWork.BookingRepository.UpdateAsync(booking);
                await _notificationService.CreateNotification(creNoti);
                await _unitOfWork.SaveAsync();
                _unitOfWork.CommitTransaction();
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
                        BookingDate = booking.BookingDate,
                        BookingId = booking.BookingId,
                        BookingTime = booking.BookingTime,
                        CreateAt = booking.CreateAt,
                        Status = booking.Status,
                        Image = booking.Bar.Images.Split(',')[0],
                        IsRated = checkIsRated,
                        BookingCode = booking.BookingCode
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
                                                          includeProperties: "Account,Bar"))
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

                if (booking.TotalPrice >= 0)
                {
                    var bookingDrinks = await _unitOfWork.BookingDrinkRepository.GetAsync(bd => bd.BookingId == booking.BookingId, includeProperties: "Drink");
                    foreach (var drink in bookingDrinks)
                    {
                        var drinkResponse = new BookingDrinkDetailResponse
                        {
                            ActualPrice = drink.ActualPrice,
                            DrinkId = drink.DrinkId,
                            DrinkName = drink.Drink.DrinkName,
                            Quantity = drink.Quantity,
                            Image = drink.Drink.Image.Split(',')[0]
                        };
                        response.bookingDrinksList.Add(drinkResponse);
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
                                                          includeProperties: "Account,Bar"))
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

                if (booking.TotalPrice >= 0)
                {
                    var bookingDrinks = await _unitOfWork.BookingDrinkRepository.GetAsync(bd => bd.BookingId == booking.BookingId, includeProperties: "Drink");
                    foreach (var drink in bookingDrinks)
                    {
                        var drinkResponse = new BookingDrinkDetailResponse
                        {
                            ActualPrice = drink.ActualPrice,
                            DrinkId = drink.DrinkId,
                            DrinkName = drink.Drink.DrinkName,
                            Quantity = drink.Quantity,
                            Image = drink.Drink.Image.Split(',')[0]
                        };
                        response.bookingDrinksList.Add(drinkResponse);
                    }
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
                        BookingCode = booking.BookingCode
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
                booking.Status = (int)BookingStatusEnum.Pending;
                booking.CreateAt = TimeHelper.ConvertDateTimeToUtcPlus7(DateTime.Now);
                var qrCode = _qrCodeService.GenerateQRCode(booking.BookingId);
                booking.QRTicket = await _firebase.UploadImageAsync(Utils.ConvertBase64ToFile(qrCode));
                booking.BookingTime = request.BookingTime;
                booking.ExpireAt = (request.BookingDate + request.BookingTime).AddHours(2);
                booking.TotalPrice = null;

                if (request.TableIds != null && request.TableIds.Count > 0)
                {
                    var existingTables = _unitOfWork.TableRepository
                                                    .Get(t => request.TableIds.Contains(t.TableId) &&
                                                              t.TableType.BarId.Equals(request.BarId),
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

                var creNoti = new NotificationRequest
                {
                    Title = booking.Bar.BarName,
                    Message = PrefixKeyConstant.BOOKING_SUCCESS,
                    BarId = booking.Bar.BarId,
                };

                try
                {
                    _unitOfWork.BeginTransaction();
                    _unitOfWork.BookingRepository.Insert(booking);
                    await _notificationService.CreateNotification(creNoti);
                    _unitOfWork.CommitTransaction();
                    await _emailSender.SendBookingInfo(booking);
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

        public async Task UpdateBookingStatus(Guid BookingId, int Status, double? AdditionalFee)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository.GetByID(accountId);

                var booking = _unitOfWork.BookingRepository.Get(b => b.BookingId == BookingId).FirstOrDefault();
                _unitOfWork.BeginTransaction();

                if (booking == null)
                {
                    throw new DataNotFoundException("Không tìm thấy đơn Đặt bàn.");
                }

                if (!getAccount.BarId.Equals(booking?.BarId))
                {
                    throw new UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (booking.Status == 1 && (Status == 2 || Status == 3))
                {
                    throw new CustomException.InvalidDataException("Không thể thực hiện check-in: Lịch đặt chỗ đã bị hủy");
                }
                if (booking.Status == 0 && booking.BookingDate.Date != DateTime.Now.Date && (Status == 2 || Status == 3))
                {
                    throw new CustomException.InvalidDataException("Không thể thực hiện check-in: vẫn chưa đến ngày đặt bàn");
                }
                if (Status == 3)
                {
                    booking.AdditionalFee = AdditionalFee == null ? 0 : (AdditionalFee < 0 ? throw new CustomException.InvalidDataException("Dịch vụ cộng thêm không thể nhỏ hơn 0") : AdditionalFee);
                }
                booking.Status = Status;
                _unitOfWork.BookingRepository.Update(booking);
                _unitOfWork.Save();

                if (Status == 2 || Status == 3)
                {
                    var bookingTables = _unitOfWork.BookingTableRepository.Get(t => t.BookingId == booking.BookingId);
                    foreach (var bookingTable in bookingTables)
                    {
                        var table = _unitOfWork.TableRepository.GetByID(bookingTable.TableId);
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

        public async Task<PaymentLink> CreateBookingTableWithDrinks(BookingDrinkRequest request, HttpContext httpContext, bool isMobile = false)
        {
            try
            {
                var booking = _mapper.Map<Booking>(request);
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
                booking.Status = (int)BookingStatusEnum.Pending;
                booking.BookingTime = request.BookingTime;
                booking.CreateAt = TimeHelper.ConvertDateTimeToUtcPlus7(DateTime.UtcNow);

                var qrCode = _qrCodeService.GenerateQRCode(booking.BookingId);
                booking.QRTicket = await _firebase.UploadImageAsync(Utils.ConvertBase64ToFile(qrCode));

                booking.ExpireAt = (request.BookingDate + request.BookingTime).AddHours(2);

                double totalPrice = 0;

                if (request.TableIds != null && request.TableIds.Count > 0)
                {
                    var existingTables = _unitOfWork.TableRepository
                                                        .Get(t => request.TableIds.Contains(t.TableId) &&
                                                                  t.TableType.BarId.Equals(request.BarId),
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

                var creNoti = new NotificationRequest
                {
                    BarId = booking.BarId,
                    Title = booking.Bar.BarName,
                    Message = PrefixKeyConstant.BOOKING_SUCCESS
                };

                try
                {
                    booking.TotalPrice = totalPrice;
                    _unitOfWork.BeginTransaction();
                    _unitOfWork.BookingRepository.Insert(booking);
                    await _notificationService.CreateNotification(creNoti);
                    _unitOfWork.CommitTransaction();
                    await _emailSender.SendBookingInfo(booking, totalPrice);
                }
                catch (Exception ex)
                {
                    _unitOfWork.RollBack();
                    throw new InternalServerErrorException($"An Internal error occurred: {ex.Message}");
                }
                return _paymentService.GetPaymentLink(booking.BookingId, booking.AccountId,
                            request.PaymentDestination, totalPrice, isMobile);
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
                                                          includeProperties: "Account,Bar"))
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

                if (booking.TotalPrice >= 0)
                {
                    var bookingDrinks = await _unitOfWork.BookingDrinkRepository.GetAsync(bd => bd.BookingId == booking.BookingId, includeProperties: "Drink");
                    foreach (var drink in bookingDrinks)
                    {
                        var drinkResponse = new BookingDrinkDetailResponse
                        {
                            ActualPrice = drink.ActualPrice,
                            DrinkId = drink.DrinkId,
                            DrinkName = drink.Drink.DrinkName,
                            Quantity = drink.Quantity,
                            Image = drink.Drink.Image.Split(',')[0]
                        };
                        response.bookingDrinksList.Add(drinkResponse);
                    }
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
    }
}
