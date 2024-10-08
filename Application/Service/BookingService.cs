﻿using Application.DTOs.Booking;
using Application.DTOs.BookingDrink;
using Application.Interfaces;
using Application.DTOs.BookingTable;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.Entities;
using Domain.IRepository;
using Domain.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IAuthentication authentication)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authentication = authentication;
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

        public async Task<BookingDetailByStaff> GetBookingDetailByStaff(Guid BookingId)
        {
            try
            {
                var response = new BookingDetailByStaff();

                var booking = (await _unitOfWork.BookingRepository.GetAsync(b => b.BookingId == BookingId, includeProperties: "Account,Bar")).FirstOrDefault();

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
                response.Note = booking.Note;

                if (booking.IsIncludeDrink)
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

        public async Task<(List<StaffBookingReponse> responses, int TotalPage, TimeSpan startTime, TimeSpan endTime)> GetListBookingByStaff(Guid BarId, string? CustomerName, string? Phone, string? Email, DateTimeOffset? bookingDate, TimeSpan? bookingTime, int? Status, int PageIndex, int PageSize)
        {
            try
            {
                var responses = new List<StaffBookingReponse>();

                var Bar = await _unitOfWork.BarRepository.GetByIdAsync(BarId);
                if (Bar == null)
                {
                    throw new CustomException.DataNotFoundException("Không có thông tin của Bar");
                }
                TimeSpan startTime = Bar.StartTime;
                TimeSpan endTime = Bar.EndTime;

                Expression<Func<Booking, bool>> filter = b =>
                b.BarId == BarId &&
                (string.IsNullOrEmpty(CustomerName) || b.Account.Fullname.Contains(CustomerName)) &&
                (string.IsNullOrEmpty(Phone) || b.Account.Phone.Contains(Phone)) &&
                (string.IsNullOrEmpty(Email) || b.Account.Email.Contains(Email)) &&
                (!bookingDate.HasValue || b.BookingDate.Date == bookingDate.Value.Date) &&
                (!bookingTime.HasValue || b.BookingTime == bookingTime.Value) &&
                (!Status.HasValue || b.Status == Status.Value);

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

                foreach (var booking in bookingsWithPagination) {
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
                    };
                    responses.Add(response);
                }

                return (responses, totalPage, startTime, endTime);
            }
            catch (Exception ex) { 
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



        public async Task UpdateBookingStatus(Guid BookingId, int Status)
        {
            try
            {
                var booking = (await _unitOfWork.BookingRepository.GetAsync(b => b.BookingId == BookingId)).FirstOrDefault();

                if (booking == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy Id Đặt bàn.");
                }

                booking.Status = Status;
                await _unitOfWork.BookingRepository.UpdateAsync(booking);
                await _unitOfWork.SaveAsync();

                if (Status == 2 || Status == 3) {
                    var bookingTables = await _unitOfWork.BookingTableRepository.GetAsync(t => t.BookingId == booking.BookingId);
                    foreach (var bookingTable in bookingTables)
                    {
                        var table = await _unitOfWork.TableRepository.GetByIdAsync(bookingTable.TableId);
                        if (table == null)
                        {
                            throw new CustomException.DataNotFoundException("Không tìm thấy bàn");
                        }
                        if(Status == 2)
                        {
                            table.Status = 1;
                        } else
                        {
                            table.Status = 0;
                        }
                        await _unitOfWork.TableRepository.UpdateAsync(table);
                        await _unitOfWork.SaveAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
