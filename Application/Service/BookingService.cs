﻿using Application.DTOs.Booking;
using Application.DTOs.BookingDrink;
using Application.IService;
using Domain.CustomException;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                    var response = new TopBookingResponse
                    {
                        BarName = booking.Bar.BarName,
                        BookingDate = booking.BookingDate,
                        BookingId = booking.BookingId,
                        BookingTime = booking.BookingTime,
                        CreateAt = booking.CreateAt,
                        Status = booking.Status,
                        Image = booking.Bar.Images.Split(',')[0]
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
                    var response = new TopBookingResponse
                    {
                        BarName = booking.Bar.BarName,
                        BookingDate = booking.BookingDate,
                        BookingId = booking.BookingId,
                        BookingTime = booking.BookingTime,
                        CreateAt = booking.CreateAt,
                        Status = booking.Status,
                        Image = booking.Bar.Images.Split(',')[0]
                    };
                    responses.Add(response);
                }

                return responses;
            } catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
