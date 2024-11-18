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

        public string GenerteDefaultToken(Guid accountId)
        {
            var getOneAccount = _unitOfWork.AccountRepository
                                        .Get(filter: x => x.AccountId.Equals(accountId),
                                        includeProperties: "Role")
                                        .FirstOrDefault();
            return _authenService.GenerteDefaultToken(getOneAccount);
        }

        public async Task<TokenResponse> GetValidRefreshToken(string token)
        {
            var accountId = _authenService.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);

            var isExitToken = (await _unitOfWork.TokenRepository
                                        .GetAsync(rt => rt.Tokens.Equals(token) &&
                                                        rt.AccountId.Equals(accountId) &&
                                                        !rt.IsRevoked && !rt.IsUsed &&
                                                        rt.Expires > DateTime.UtcNow))
                                        .FirstOrDefault();
            if (isExitToken == null)
            {
                throw new CustomException.DataNotFoundException("Không tìm thấy token !");
            }

            var response = _mapper.Map<TokenResponse>(isExitToken);
            return response;
        }

        public async Task<bool> IsValidRefreshToken(string refreshToken)
        {
            var token = (await _unitOfWork.TokenRepository.GetAsync(rt => rt.Tokens == refreshToken)).FirstOrDefault();

            return token != null && !token.IsRevoked && token.Expires > DateTime.UtcNow;
        }

        public async Task<bool> RevokeRefreshToken(string token)
        {
            try
            {
                var accountId = _authenService.GetUserIdFromHttpContext(_httpContextAccessor.HttpContext);

                var isExistToken = _unitOfWork.TokenRepository
                                                .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                        x.Tokens.Equals(token))
                                                .FirstOrDefault();
                if (isExistToken == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy tokens");
                }

                if (token != null)
                {
                    isExistToken.IsRevoked = true;
                    await _unitOfWork.TokenRepository.UpdateRangeAsync(isExistToken);
                    await Task.Delay(10);
                    await _unitOfWork.SaveAsync();
                }
                return true;
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
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
                    Expires = DateTime.Now.AddMonths(1),
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
