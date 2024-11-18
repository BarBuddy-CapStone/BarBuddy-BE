
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
        Task<TokenResponse> SaveRefreshToken(string token, Guid accountId);
        Task<TokenResponse> GetValidRefreshToken(string token);
        Task<bool> RevokeRefreshToken(string token);
        Task<bool> IsValidRefreshToken(string refreshToken);
        string GenerteDefaultToken(Guid accountId);
    }
}
