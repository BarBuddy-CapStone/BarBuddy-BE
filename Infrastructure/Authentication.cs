using Application.Interfaces;
using Domain.CustomException;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure
{
    public class Authentication : IAuthentication
    {
        public async Task<string> HashedPassword(string password)
        {
            try
            {
                using (SHA512 sha512 = SHA512.Create())
                {
                    byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));

                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        stringBuilder.Append(hashBytes[i].ToString("x2"));
                    }
                    return await Task.FromResult<string?>(stringBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string GenerteDefaultToken(Account account)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];
            var key = config["Jwt:Key"];

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, account.RoleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, account.Email.ToString()),
                new Claim(ClaimTypes.Role, account.Role.RoleName),
                new Claim("id", account.AccountId.ToString()),
            };

            var expired = DateTime.UtcNow.AddMinutes(30);

            var token = new JwtSecurityToken(issuer, audience, claims, notBefore: DateTime.UtcNow, expired, credentials);
            return jwtSecurityTokenHandler.WriteToken(token);
        }

        public Guid GetUserIdFromHttpContext(HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                throw new CustomException.InternalServerErrorException("Authorization header is missing.");
            }

            string authorizationHeader = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new CustomException.InternalServerErrorException("Invalid Authorization header format.");
            }

            string jwtToken = authorizationHeader["Bearer ".Length..];

            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(jwtToken))
            {
                throw new CustomException.InternalServerErrorException("Invalid JWT token format.");
            }

            try
            {
                var token = tokenHandler.ReadJwtToken(jwtToken);
                var idClaim = token.Claims.FirstOrDefault(claim => claim.Type == "id");

                if (idClaim == null || string.IsNullOrWhiteSpace(idClaim.Value))
                {
                    throw new CustomException.InternalServerErrorException("User ID claim not found in token.");
                }

                return Guid.Parse(idClaim.Value);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException($"Error parsing token: {ex.Message}");
            }
        }
    }
}
