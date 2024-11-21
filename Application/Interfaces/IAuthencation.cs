using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthentication
    {
        Task<string> HashedPassword(string password);
        string GenerteDefaultToken(Account account);
        Guid GetUserIdFromHttpContext(HttpContext httpContext);
        string GenerateRefreshToken(Account account);
    }
}
