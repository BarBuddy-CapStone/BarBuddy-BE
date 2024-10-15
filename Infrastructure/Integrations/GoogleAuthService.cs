using Application.DTOs.Authen;
using Application.Interfaces;
using Firebase.Storage;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Integrations
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IFirebase _firebaseService;

        public GoogleAuthService(IConfiguration configuration, HttpClient httpClient, IFirebase firebaseService)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _firebaseService = firebaseService;
        }

        public async Task<GoogleUserRequest> AuthenticateGoogleUserAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserRequest
            {
                Name = payload.Name,
                Email = payload.Email,
                PictureUrl = payload.Picture
            };
        }

        public async Task<string> UploadProfilePictureAsync(string pictureUrl, string userId)
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(pictureUrl);
            var fileName = $"{userId}_profile_picture.jpg";

            using (var ms = new MemoryStream(imageBytes))
            {
                var formFile = new FormFile(ms, 0, imageBytes.Length, "profile_picture", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                };

                return await _firebaseService.UploadImageAsync(formFile);
            }
        }
    }
}
