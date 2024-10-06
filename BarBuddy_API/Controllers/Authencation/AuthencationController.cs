using Application.DTOs.Authen;
using Application.Interfaces;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BarBuddy_API.Controllers.Authencation
{
    [ApiController]
    [Route("api/authen")]
    public class AuthencationController : BaseController
    {
        private readonly IAuthenService _authenService;
        private readonly IOtpSender _otpSender;

        public AuthencationController(IAuthenService authenService, IOtpSender otpSender)
        {
            _authenService = authenService;
            _otpSender = otpSender;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authenService.Login(request);
                return CustomResult("Đăng nhập thành công !", response);
            }
            catch (CustomException.InternalServerErrorException ex) {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authenService.Register(request);
                return CustomResult("Đăng kí thành công !", response);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            await _otpSender.SendOtpAsync(email);
            return Ok("OTP đã được gửi thành công.");
        }

        [HttpPost("verify")]
        public IActionResult VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            bool isValid = _otpSender.VerifyOtp(request);

            return isValid ? Ok("OTP hợp lệ.") : BadRequest("OTP không hợp lệ hoặc đã hết hạn.");
        }
    }
}
