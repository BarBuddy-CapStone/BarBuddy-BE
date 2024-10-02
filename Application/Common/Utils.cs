using Domain.CustomException;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Application.Common
{
    public static class Utils
    {
        public static List<IFormFile> CheckValidateImageFile(List<IFormFile> formImages)
        {
            List<IFormFile> imgsFile = new List<IFormFile>();
            if (!formImages.IsNullOrEmpty()) {
                foreach (var image in formImages)
                {
                    if (image.Length > 10 * 1024 * 1024)
                    {
                        throw new CustomException.InvalidDataException("File size exceeds the maximum allowed limit.");
                    }
                    imgsFile.Add(image);
                }
            }
            return imgsFile;
        }

        public static IFormFile CheckValidateSingleImageFile(IFormFile formImage)
        {
            if (formImage.Length > 10 * 1024 * 1024)
            {
                throw new CustomException.InvalidDataException("File size exceeds the maximum allowed limit.");
            }
            return formImage;
        }
    }
}
