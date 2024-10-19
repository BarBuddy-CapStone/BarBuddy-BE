using Application.DTOs.Authen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserRequest> AuthenticateGoogleUserAsync(string idToken);
        Task<string> UploadProfilePictureAsync(string pictureUrl, string userId);
    }
}
