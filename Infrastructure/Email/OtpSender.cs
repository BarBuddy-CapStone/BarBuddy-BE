using Application.DTOs.Authen;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Email
{
    public class OtpSender : IOtpSender
    {
        private readonly IMemoryCache _cache;
        private readonly IEmailSender _emailSender;

        public OtpSender(IMemoryCache cache, IEmailSender emailSender)
        {
            _cache = cache;
            _emailSender = emailSender;
        }

        public async Task SendOtpAsync(string email)
        {
            string otp = GenerateOtp();
            TimeSpan expireTime = TimeSpan.FromMinutes(3);
            _cache.Set(email, otp, expireTime);

            await _emailSender.SendEmail(email, "OTP Code", $"Your OTP code is {otp}. It will expire in {expireTime.Minutes} minutes.");
        }

        public bool VerifyOtp(OtpVerificationRequest request)
        {
            return _cache.TryGetValue(request.Email, out string cachedOtp) && cachedOtp == request.Otp;
        }

        private string GenerateOtp()
        {
            return new Random().Next(000000, 999999).ToString();
        }
    }
}
