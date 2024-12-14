using Application.DTOs.Account;
using Application.DTOs.Authen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IAuthenService
    {
        Task<LoginResponse> Login(LoginRequest request);
        Task<LoginResponse> GoogleLogin(string idToken);
        Task<CustomerAccountResponse> Register(RegisterRequest request);
        Task<bool> ConfirmAccountByOtp(OtpVerificationRequest request);
        Task<bool> RegisterWithOtp(RegisterRequest request);
        Task<bool> ResetPassword(string email);
        string VerifyResetPassword(OtpVerificationRequest request);
        Task<bool> ResetToNewPassword(ResetPasswordRequest request); 
    }
}
