using Azure.Core;
using Domain.CustomException;
using Domain.Entities;
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

        public static void ValidateOpenCloseTime(DateTimeOffset requestDate, TimeSpan requestTime, List<BarTime> barTimes)
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
                    throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!");
                }
            }

            bool isValidTime = barTimes.Any(barTime => 
                (requestTime >= barTime.StartTime && requestTime <= barTime.EndTime));

            if (!isValidTime)
            {
                throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!");
            }
        }

        public static void ValidateEventTime(DateTimeOffset? requestDate, TimeSpan startTime, TimeSpan endTime, TimeSpan openTime, TimeSpan closeTime)
        {
            if (requestDate != null)
            {
                ValidateDateNotInPast(requestDate.Value);
                //ValidateOpenCloseTime(requestDate.Value, startTime, openTime, closeTime);
                //ValidateOpenCloseTime(requestDate.Value, endTime, openTime, closeTime);
            }
            else
            {
                ValidateTimeWithinRange(startTime, openTime, closeTime, "Thời gian bắt đầu");
                ValidateTimeWithinRange(endTime, openTime, closeTime, "Thời gian kết thúc");
            }

            ValidateEndTimeAfterStartTime(startTime, endTime);
        }

        private static void ValidateDateNotInPast(DateTimeOffset requestDate)
        {
            var currentDate = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now.Date);
            if (requestDate < currentDate)
            {
                throw new CustomException.InvalidDataException("Không hợp lệ");
            }
        }

        private static void ValidateTimeWithinOpenClose(DateTimeOffset requestDate, TimeSpan time, TimeSpan openTime, TimeSpan closeTime)
        {
            var currentTimeOfDay = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.UtcNow).TimeOfDay;

            if (requestDate == TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now.Date) && time < currentTimeOfDay)
            {
                throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!");
            }

            ValidateTimeWithinRange(time, openTime, closeTime, "Thời gian");
        }

        private static void ValidateTimeWithinRange(TimeSpan time, TimeSpan openTime, TimeSpan closeTime, string timeLabel)
        {
            if (time > closeTime && time < TimeSpan.FromHours(24))
            {
                if (time < openTime)
                {
                    throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar !");
                }
            }
        }

        private static void ValidateEndTimeAfterStartTime(TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime <= startTime)
            {
                throw new CustomException.InvalidDataException("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            }
        }

        public static IFormFile ConvertBase64ToFile(string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            var stream = new MemoryStream(bytes);

            var file = new FormFile(stream, 0, bytes.Length, "file", "undefined_name.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };

            return file;
        }

        public static List<IFormFile> ConvertBase64ListToFiles(List<string> base64Strings)
        {
            var files = new List<IFormFile>();

            foreach (var base64String in base64Strings)
            {
                var file = ConvertBase64ToFile(base64String);
                files.Add(file);
            }

            return files;
        }
    }
}
