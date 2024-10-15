using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmail(string toEmail, string subject, string message);
        //Task SendBookingInfo(Guid bookingId, double totalPrice = 0);
        Task SendBookingInfo(Booking booking, double totalPrice = 0);
    }
}
