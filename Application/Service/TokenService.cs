using Application.DTOs.Authen;
using Application.DTOs.Tokens;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Azure.Core;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.CustomException.CustomException;

namespace Application.Service
{
    public class TokenService : ITokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAuthentication _authenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IUnitOfWork unitOfWork, IMapper mapper,
                            IAuthentication authenService, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authenService = authenService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResponse> GenerteDefaultToken(string refreshToken)
        {
            var accountId = _authenService.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);
            await IsValidRefreshToken(refreshToken);
            var isValidToken = await GetValidRefreshToken(accountId, refreshToken);

            if (isValidToken == null)
            {
                throw new UnAuthorizedException("Refresh token không hợp lệ hoặc đã hết hạn!");
            }

            var account = _unitOfWork.AccountRepository
                                        .Get(filter: x => x.AccountId.Equals(accountId), 
                                        includeProperties: "Role")
                                        .FirstOrDefault();
            if (account == null)
            {
                throw new DataNotFoundException("Không tìm thấy tài khoản!");
            }

            var newAccessToken = _authenService.GenerteDefaultToken(account);

            var response = _mapper.Map<LoginResponse>(account);
            response.AccessToken = newAccessToken;
            response.RefreshToken = refreshToken;

            return response;
        }

        public async Task<TokenResponse> GetValidRefreshToken(Guid accountId, string refreshToken)
        {
            var isExitToken = (await _unitOfWork.TokenRepository
                                        .GetAsync(rt => rt.Tokens.Equals(refreshToken) &&
                                                        rt.AccountId.Equals(accountId) &&
                                                        !rt.IsRevoked && !rt.IsUsed &&
                                                        rt.Expires > DateTime.UtcNow))
                                        .FirstOrDefault();
            if (isExitToken == null)
            {
                throw new DataNotFoundException("Không tìm thấy token !");
            }

            var response = _mapper.Map<TokenResponse>(isExitToken);
            return response;
        }

        public async Task<bool> IsValidRefreshToken(string refreshToken)
        {
            var accountId = _authenService.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);
            var token = (await _unitOfWork.TokenRepository.GetAsync(rt => rt.Tokens == refreshToken)).FirstOrDefault();
            if (token == null)
                throw new UnAuthorizedException("Refresh token không tồn tại !");

            if (token.IsUsed || token.IsRevoked)
                throw new UnAuthorizedException("Refresh token đã hết hạn hoặc đã dùng");

            if (token.Expires < DateTime.Now)
                throw new UnAuthorizedException("Refresh token đã hết hạn");

            return token != null && !token.IsRevoked && token.Expires > DateTime.UtcNow;
        }

        public async Task<bool> RevokeRefreshToken(string refreshToken)
        {
            try
            {
                await IsValidRefreshToken(refreshToken);

                var accountId = _authenService.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);
                var isExistToken = _unitOfWork.TokenRepository
                                                .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                        x.Tokens.Equals(refreshToken))
                                                .FirstOrDefault();
                if (isExistToken == null)
                {
                    throw new DataNotFoundException("Không tìm thấy tokens");
                }
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return false;
                }

                isExistToken.IsRevoked = true;
                await _unitOfWork.TokenRepository.UpdateRangeAsync(isExistToken);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch
            {
                throw new InternalServerErrorException("Lỗi hệ thống !");
            }
        }

        public async Task<TokenResponse> SaveRefreshToken(string token, Guid accountId)
        {
            try
            {

                var reftoken = new Token
                {
                    AccountId = accountId,
                    Tokens = token,
                    Created = DateTime.UtcNow,
                    Expires = DateTime.Now.AddDays(14),
                    IsRevoked = false,
                    IsUsed = false
                };
                await _unitOfWork.TokenRepository.InsertAsync(reftoken);
                await Task.Delay(10);
                await _unitOfWork.SaveAsync();

                var response = _mapper.Map<TokenResponse>(reftoken);
                return response;
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }


    }
}
