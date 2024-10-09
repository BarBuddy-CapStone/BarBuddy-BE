using Application.DTOs.Booking;
using Application.DTOs.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IBookingService
    {
        Task<(List<TopBookingResponse> responses, int TotalPage)> GetAllCustomerBooking(Guid CustomerId, int? Status, int PageIndex = 1, int PageSize = 10);
        Task<List<TopBookingResponse>> GetTopBookingByCustomer(Guid CustomerId, int NumOfBookings);
        Task<BookingByIdResponse> GetBookingById(Guid BookingId);
        Task<BookingDetailByStaff> GetBookingDetailByStaff(Guid BookingId);
        Task<(List<StaffBookingReponse> responses, int TotalPage, TimeSpan startTime, TimeSpan endTime)> GetListBookingByStaff(Guid BarId, string? CustomerName, string? Phone, string? Email, DateTimeOffset? bookingDate, TimeSpan? bookingTime, int? Status, int PageIndex, int PageSize);
        Task UpdateBookingStatus(Guid BookingId, int Status);
        Task<bool> CancelBooking(Guid BookingId);
        BookingResponse CreateBookingTableOnly(BookingTableRequest request, HttpContext httpContext);
        PaymentLink CreateBookingTableWithDrinks(BookingDrinkRequest request, HttpContext httpContext);
    }
}
