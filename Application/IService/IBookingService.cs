using Application.DTOs.Booking;
using Application.DTOs.BookingDrink;
using Application.DTOs.BookingExtraDrink;
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
        Task<BookingDetailByStaff> GetBookingDetailAuthorized(Guid BookingId);
        Task<(List<StaffBookingReponse> responses, int TotalPage)> GetListBookingAuthorized(string qrTicket, Guid BarId, string? CustomerName, string? Phone, string? Email, DateTimeOffset? bookingDate, TimeSpan? bookingTime, int? Status, int PageIndex, int PageSize);
        Task UpdateBookingStatus(Guid BookingId, int Status);
        Task<bool> CancelBooking(Guid BookingId);
        Task<BookingResponse> CreateBookingTableOnly(BookingTableRequest request, HttpContext httpContext);
        Task<PaymentLink> CreateBookingTableWithDrinks(BookingDrinkRequest request, HttpContext httpContext, bool isMobile = false);
        Task<List<BookingCustomResponse>> GetAllBookingByStsPending();
        Task<List<BookingCustomResponse>> GetAllBookingByStsPendingCus();
        Task UpdateStsBookingServing(Guid bookingId);
        Task<(List<StaffBookingReponse> responses, int TotalPage)> GetListBookingAuthorizedAdmin(string qrTicket, Guid? BarId, string? CustomerName, string? Phone, string? Email, DateTimeOffset? bookingDate, TimeSpan? bookingTime, int? Status, int PageIndex, int PageSize);
        Task<BookingDetailByStaff> GetBookingDetailAuthorizedAdmin(Guid BookingId);
        Task<List<BookingDrinkDetailResponse>> ExtraDrinkInServing(Guid bookingId, List<DrinkRequest> request);
        Task<List<BookingDrinkDetailResponse>> UpdExtraDrinkInServing(Guid bookingId, List<UpdBkDrinkExtraRequest> request);
        Task<List<BookingDrinkDetailResponse>> GetExtraBookingServing(Guid bookingId);
    }
}
