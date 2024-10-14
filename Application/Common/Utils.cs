using Azure.Core;
using Domain.CustomException;
using Domain.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
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
            if (!formImages.IsNullOrEmpty())
            {
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
        public static ValidationResult? ValidateAge(DateTimeOffset birthdate, ValidationContext context)
        {
            DateTimeOffset today = DateTimeOffset.Now;

            int age = today.Year - birthdate.Year;

            if (birthdate > today.AddYears(-age))
            {
                age--;
            }

            return age >= 18 ? ValidationResult.Success : new ValidationResult("Bạn phải đủ 18 tuổi.");
        }

        public static void ValidateOpenCloseTime(DateTimeOffset requestDate, TimeSpan requestTime, TimeSpan openTime, TimeSpan closeTime)
        {
            var currentDate = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now.Date);
            if (requestDate < currentDate)
            {
                throw new CustomException.InvalidDataException("Không hợp lệ");
            }
            else if (requestDate == currentDate)
            {
                if (requestTime < TimeHelper.ConvertToUtcPlus7(DateTimeOffset.UtcNow).TimeOfDay)
                {
                    throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar !");
                }
                if (requestTime < openTime || requestTime > TimeSpan.FromHours(23.9999))
                {
                    if (requestTime > closeTime)
                    {
                        throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar !");
                    }
                }
            }
            else if (requestDate > currentDate)
            {
                if (requestTime > closeTime && requestTime < TimeSpan.FromHours(24))
                {
                    if (requestTime < openTime)
                    {
                        throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar !");
                    }
                }
            }
        }
    }
}
