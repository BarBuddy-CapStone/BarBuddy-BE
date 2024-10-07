using Application.DTOs.Booking;
using Application.DTOs.BookingDrink;
using Application.DTOs.Payment;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Domain.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.CustomException.CustomException;

namespace Application.Service
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthentication _authentication;
        private readonly IPaymentService _paymentService;

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IAuthentication authentication, IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authentication = authentication;
            _paymentService = paymentService;
        }

        public async Task<bool> CancelBooking(Guid BookingId)
        {
            try
            {
                var booking = (await _unitOfWork.BookingRepository.GetAsync(b => b.BookingId == BookingId, includeProperties: "Account,Bar")).FirstOrDefault();

                if (booking == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy Id Đặt bàn.");
                }

                if (booking.Status != 0)
                {
                    throw new CustomException.DataNotFoundException("Bạn không thể hủy đặt bàn.");
                }

                DateTime BookingDateTime = booking.BookingDate.DateTime + booking.BookingTime;
                if (BookingDateTime < DateTime.Now.AddHours(2))
                {
                    return false;
                }

                // Cancelled status (temp)
                booking.Status = 1;

                await _unitOfWork.BookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveAsync();
                return true;
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
                        } else
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
                        IsRated = checkIsRated
                    };
                    responses.Add(response);
                }

                return (responses, TotalPage);
            }
            catch (Exception ex) {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<BookingByIdResponse> GetBookingById(Guid BookingId)
        {
            try
            {
                var response = new BookingByIdResponse();

                var booking = (await _unitOfWork.BookingRepository.GetAsync(b => b.BookingId == BookingId, includeProperties: "Account,Bar")).FirstOrDefault();

                if(booking == null)
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
                response.Note = booking.Note;
                response.Images = booking.Bar.Images.Split(',').ToList();

                if (booking.IsIncludeDrink) {
                    var bookingDrinks = await _unitOfWork.BookingDrinkRepository.GetAsync(bd => bd.BookingId == booking.BookingId, includeProperties: "Drink");
                    foreach (var drink in bookingDrinks) {
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
                foreach (var table in bookingTables) { 
                    response.TableNameList.Add(table.Table.TableName);
                }

                return response;
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
                var responses = new List<TopBookingResponse>();

                var bookings = await _unitOfWork.BookingRepository.GetAsync(b => b.AccountId == CustomerId, pageIndex: 1, pageSize: NumOfBookings, orderBy: o => o.OrderByDescending(b => b.CreateAt).ThenByDescending(b => b.BookingDate), includeProperties: "Bar");
                foreach (var booking in bookings) {
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
                        IsRated = checkIsRated
                    };
                    responses.Add(response);
                }

                return responses;
            } catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public BookingResponse CreateBookingTableOnly(BookingTableRequest request, HttpContext httpContext)
        {
            try
            {
                var booking = _mapper.Map<Booking>(request);
                booking.Account = _unitOfWork.AccountRepository.GetByID(_authentication.GetUserIdFromHttpContext(httpContext)) ?? throw new DataNotFoundException("account not found");
                booking.BookingTables = booking.BookingTables ?? new List<BookingTable>();
                booking.BookingCode = $"BOOKING-{RandomHelper.GenerateRandomNumberString()}";
                booking.Status = (int)PaymentStatusEnum.Pending;
                booking.IsIncludeDrink = false; 

                if (request.TableIds != null && request.TableIds.Count > 0)
                {
                    var existingTables = _unitOfWork.TableRepository.Get(t => request.TableIds.Contains(t.TableId) && t.BarId == request.BarId);
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
                            ReservationDate = request.BookingDate,
                            ReservationTime = request.BookingTime
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
            catch (CustomException.InvalidDataException ex)
            {
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public PaymentLink CreateBookingTableWithDrinks(BookingDrinkRequest request, HttpContext httpContext)
        {
            try
            {
                var booking = _mapper.Map<Booking>(request);
                booking.Account = _unitOfWork.AccountRepository.GetByID(_authentication.GetUserIdFromHttpContext(httpContext)) ?? throw new DataNotFoundException("account not found");
                booking.Bar = _unitOfWork.BarRepository.GetByID(request.BarId) ?? throw new DataNotFoundException("Bar not found");
                booking.BookingTables = booking.BookingTables ?? new List<BookingTable>();
                booking.BookingDrinks = booking.BookingDrinks ?? new List<BookingDrink>();
                booking.BookingCode = $"BOOKING-{RandomHelper.GenerateRandomNumberString()}";
                booking.Status = (int)PaymentStatusEnum.Pending;
                booking.IsIncludeDrink = true;

                double totalPrice = 0;

                if (request.TableIds != null && request.TableIds.Count > 0)
                {
                    var existingTables = _unitOfWork.TableRepository.Get(t => request.TableIds.Contains(t.TableId) && t.BarId == request.BarId);
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
                            ReservationDate = request.BookingDate,
                            ReservationTime = request.BookingTime
                        };

                        booking.BookingTables?.Add(bookingTable);
                    }
                }
                if (request.Drinks != null && request.Drinks.Count > 0)
                {
                    var drinkIds = request.Drinks.Select(drink => drink.DrinkId).ToList();
                    var existingDrinks = _unitOfWork.DrinkRepository.Get(d => drinkIds.Contains(d.DrinkId));
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
                            ActualPrice = existingDrinks.FirstOrDefault(e => e.DrinkId == drink.DrinkId).Price,
                            Quantity = drink.Quantity
                        };
                        totalPrice += bookingDrink.ActualPrice * bookingDrink.Quantity;
                        booking.BookingDrinks?.Add(bookingDrink);
                    }
                    totalPrice = totalPrice - totalPrice * booking.Bar.Discount/100;
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
                }
                catch (Exception ex)
                {
                    _unitOfWork.RollBack();
                    throw new InternalServerErrorException($"An Internal error occurred while creating customer: {ex.Message}");
                }
                return _paymentService.GetPaymentLink(booking.BookingId, booking.AccountId, request.PaymentDestination, totalPrice);
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
    }
}
