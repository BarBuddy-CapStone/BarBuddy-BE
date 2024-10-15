﻿using Application.DTOs.Authen;
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

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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
        /// Register Account
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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
        /// SendOtp From Email Bar to Email Register
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
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
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            bool isValid = await _authenService.ConfirmAccountByOtp(request);

            return isValid ? CustomResult("OTP hợp lệ.") : CustomResult("OTP không hợp lệ hoặc đã hết hạn.", HttpStatusCode.BadRequest);
        }
    }
}
