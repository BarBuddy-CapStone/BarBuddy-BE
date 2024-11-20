
using Application.DTOs.Authen;
using Application.DTOs.Tokens;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface ITokenService
    {
        Task<TokenResponse> SaveRefreshToken(string refreshToken, Guid accountId);
        Task<TokenResponse> GetValidRefreshToken(Guid accountId, string refreshToken);
        Task<bool> RevokeRefreshToken(string refreshToken);
        Task<bool> IsValidRefreshToken(string refreshToken);
        Task<LoginResponse> GenerteDefaultToken(string refreshToken);
    }
}
