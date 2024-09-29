using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuthentication
    {
        //string GenerateJWTToken(User user);
        Guid GetUserIdFromHttpContext(HttpContext httpContext);

        //bool VerifyPassword(string providedPassword, string hashedPassword, User user);

        //string HashPassword(User user, string password);
    }
}
