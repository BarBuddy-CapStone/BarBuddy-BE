using Application.DTOs.Authen;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using static QRCoder.PayloadGenerator;

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

        public async Task SendOtpResetPasswordAsync(string email)
        {
            string otp = GenerateOtp();
            TimeSpan expireTime = TimeSpan.FromMinutes(3);
            string keyCache = $"RESET-PASSWORD-{email}";
            _cache.Set(keyCache, otp, expireTime);

            await _emailSender.SendEmail(email, "OTP Code", $"Your OTP code is {otp}. It will expire in {expireTime.Minutes} minutes.");
        }

        public bool VerifyOtp(OtpVerificationRequest request)
        {
            return _cache.TryGetValue(request.Email, out string cachedOtp) && cachedOtp == request.Otp;
        }

        public bool VerifyOtpResetPassword(OtpVerificationRequest request)
        {
            string keyCache = $"RESET-PASSWORD-{request.Email}";
            return _cache.TryGetValue(keyCache, out string cachedOtp) && cachedOtp == request.Otp;
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
