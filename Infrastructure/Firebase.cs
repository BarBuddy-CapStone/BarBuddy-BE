using Application.Interfaces;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Firebase : IFirebase
    {
        private readonly IConfiguration _configuration;

        public Firebase(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var _apiKey = _configuration["Firebase:ApiKey"];
            var _storage = _configuration["Firebase:Storage"];
            var _authEmail = _configuration["Firebase:AuthEmail"];
            var _authPassword = _configuration["Firebase:AuthPassword"];
            var auth = new FirebaseAuthProvider(new FirebaseConfig(_apiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(_authEmail, _authPassword);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            string folderName;

            var fileExtension = Path.GetExtension(file.FileName);

            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                    folderName = "images";
                    break;
                case ".docx":
                    folderName = "docx";
                    break;
                case ".ppt":
                case ".pptx":
                    folderName = "ppt";
                    break;
                case ".mp4":
                case ".mov":
                    folderName = "videos";
                    break;
                default:
                    folderName = "other";
                    break;
            }

            var storage = new FirebaseStorage(_storage, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                ThrowOnCancel = true
            });

            using (var stream = file.OpenReadStream())
            {
                var storageReference = storage.Child(folderName).Child(fileName);
                await storageReference.PutAsync(stream);

                return await storageReference.GetDownloadUrlAsync();
            }
        }
    }
}
