using Application.DTOs.Authen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOtpSender
    {
        Task SendOtpAsync(string email);
        bool VerifyOtp(OtpVerificationRequest request);
    }
}
