using Domain.CustomException;
using Domain.Interfaces;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Authentication1
{
    public class Authen : IAuthentication
    {
        //private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Authen> _logger;

        public Authen(IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<Authen> logger)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            //_userManager = userManager;
            //_logger = logger; _userManager = userManager;
        }

        //private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        //public bool VerifyPassword(string providedPassword, string hashedPassword, User user)
        //{
        //    var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
        //    return result == PasswordVerificationResult.Success;
        //}
        //public string HashPassword(User user, string password)
        //{
        //    return _passwordHasher.HashPassword(user, password);
        //}

        //public string GenerateJWTToken(User user)
        //{
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //    var roles = _userManager.GetRolesAsync(user).Result;

        //    var claims = new[]
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        new Claim("id", user.Id.ToString()),
        //        new Claim(ClaimTypes.Role, string.Join(",", roles)),
        //        new Claim("name", user.Name),
        //        new Claim("avatar",user.Avatar)
        //    };

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.Now.AddDays(1),
        //        signingCredentials: credentials
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

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
