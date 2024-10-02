﻿using Application.DTOs.Booking;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IBookingService
    {
        Task<(List<AllCustomerBookingResponse> responses, int TotalPage)> GetAllCustomerBooking(Guid CustomerId, int? Status, int PageIndex = 1, int PageSize = 10);
        Task<List<TopBookingResponse>> GetTopBookingByCustomer(Guid CustomerId, int NumOfBookings);
        Task<BookingByIdResponse> GetBookingById(Guid BookingId);
        Task<bool> CancelBooking(Guid BookingId);
    }
}
