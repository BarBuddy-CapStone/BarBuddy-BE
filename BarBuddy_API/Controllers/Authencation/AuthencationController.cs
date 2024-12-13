using Application.DTOs.Authen;
using Application.Interfaces;
using Application.IService;
using CoreApiResponse;
using Domain.CustomException;
using Firebase.Auth;
using Infrastructure.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace BarBuddy_API.Controllers.Authencation
{
    [ApiController]
    [Route("api/authen")]
    public class AuthencationController : BaseController
    {
        private readonly IAuthenService _authenService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IOtpSender _otpSender;
        private readonly ITokenService _tokenService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthentication _authentication;
        public AuthencationController(IAuthenService authenService, IOtpSender otpSender, 
                                        IGoogleAuthService googleAuthService, ITokenService tokenService, 
                                        IHttpContextAccessor contextAccessor, IAuthentication authentication)
        {
            _authenService = authenService;
            _otpSender = otpSender;
            _googleAuthService = googleAuthService;
            _tokenService = tokenService;
            _contextAccessor = contextAccessor;
            _authentication = authentication;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
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

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="idToken"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody][Required] GoogleLoginRequest request)
        {
            try
            {
                var response = await _authenService.GoogleLogin(request.IdToken);
                return CustomResult("Đăng nhập thành công !", response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xác thực: {ex.Message}");
            }
        }

        /// <summary>
        /// Register Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                
                var response = await _authenService.RegisterWithOtp(request);
                return response ? CustomResult("Đăng kí thành công ! Bạn có 2 phút để nhập OTP", response) : 
                    CustomResult("Đăng kí không thành công !", HttpStatusCode.BadRequest);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// ResetPassword From Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] string email)
        {
            var response = await _authenService.ResetPassword(email);
            if(response)
            {
                return CustomResult("Đặt lại mật khẩu thành công", "Quý khách hãy check Email hoặc tin Spam");
            } else
            {
                return CustomResult("Đặt lại mật khẩu không thành công");
            }
        }

        [AllowAnonymous]
        [HttpPost("reset-password/verification")]
        public async Task<IActionResult> ResetPasswordVerification([FromBody] OtpVerificationRequest request)
        {
            var response = await _authenService.ResetPasswordVerification(request);
            if (response != null)
            {
                return CustomResult("Xác thực đặt lại mật khẩu thành công", response);
            }
            else
            {
                return CustomResult("Xác thực đặt lại mật khẩu không thành công", code: HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// SendOtp From Email Bar to Email Register
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("send")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            await _otpSender.SendOtpAsync(email);
            return Ok("OTP đã được gửi thành công.");
        }

        /// <summary>
        /// Verify Otp sended to Email Register
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            bool isValid = await _authenService.ConfirmAccountByOtp(request);

            return isValid ? CustomResult("OTP hợp lệ.") : CustomResult("OTP không hợp lệ hoặc đã hết hạn.", HttpStatusCode.BadRequest);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody]string refreshToken)
        {
            try
            {
                var response = await _tokenService.GenerteDefaultToken(refreshToken);
                return CustomResult("Đã refresh token thành công !", response);
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.NotFound);
            }
            catch (CustomException.InvalidDataException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.BadRequest);
            }
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string token) 
        {
            try
            {
                await _tokenService.IsValidRefreshToken(token);       
                var result = await _tokenService.RevokeRefreshToken(token);

                if (!result)
                {
                    return CustomResult("Token không tồn tại hoặc đã hết hạn !", HttpStatusCode.BadRequest);
                }

                return CustomResult("Đã logout thành công !");
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                return CustomResult(ex.Message, HttpStatusCode.Unauthorized);
            }
        }
    }
}
