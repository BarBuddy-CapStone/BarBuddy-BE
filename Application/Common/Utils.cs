using Application.DTOs.Events.EventTime;
using Azure.Core;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
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

            bool isValidTime = barTimes.Any(barTime =>
            {
                if (barTime.StartTime < barTime.EndTime)
                {
                    return requestTime >= barTime.StartTime && requestTime <= barTime.EndTime;
                }
                else
                {
                    return requestTime >= barTime.StartTime || requestTime <= barTime.EndTime;
                }
            });

            if (!isValidTime)
            {
                throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!");
            }
        }
        public static void ValidateEventTime(DateTimeOffset? requestDate, TimeSpan startTime, TimeSpan endTime, List<BarTime> barTimes)
        {
            if (requestDate != null)
            {
                ValidateDateNotInPast(requestDate.Value);
                ValidateOpenCloseTime(requestDate.Value, startTime, barTimes);
                ValidateOpenCloseTime(requestDate.Value, endTime, barTimes);
            }
            else
            {
                ValidateTimeWithinRange(startTime, barTimes, "Thời gian bắt đầu");
                ValidateTimeWithinRange(endTime, barTimes, "Thời gian kết thúc");
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
        private static void ValidateTimeWithinOpenClose(DateTimeOffset requestDate, TimeSpan time, List<BarTime> barTimes)
        {
            var currentTimeOfDay = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.UtcNow).TimeOfDay;

            if (requestDate == TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now.Date) && time < currentTimeOfDay)
            {
                throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!");
            }

            ValidateTimeWithinRange(time, barTimes, "Thời gian");
        }
        public static void ValidateTimeWithinEvent(DateTimeOffset requestDate, TimeSpan time, List<TimeEvent> eventTimes)
        {
            var currentTimeOfDay = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.UtcNow).TimeOfDay;

            if (requestDate == TimeHelper.ConvertToUtcPlus7(DateTimeOffset.Now.Date))
            {
                throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ diễn ra sự kiện của quán Bar!");
            }

            ValidateTimeWithinRangeEvent(time, eventTimes, "Thời gian");
        }
        private static void ValidateTimeWithinRange(TimeSpan time, List<BarTime> barTimes, string timeLabel)
        {
            bool isValidTime = barTimes.Any(barTime =>
            {
                if (barTime.StartTime < barTime.EndTime)
                {
                    return time >= barTime.StartTime && time <= barTime.EndTime;
                }
                else
                {
                    return time >= barTime.StartTime || time <= barTime.EndTime;
                }
            });

            if (!isValidTime)
            {
                throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!");
            }
        }
        public static bool ValidateTimeWithinRangeEvent(TimeSpan time, List<TimeEvent> eventTimes, string timeLabel)
        {
            bool isValidTime = eventTimes.Any(eventTime =>
            {
                if (eventTime.StartTime < eventTime.EndTime)
                {
                    return time >= eventTime.StartTime && time <= eventTime.EndTime;
                }
                else
                {
                    return time >= eventTime.StartTime || time <= eventTime.EndTime;
                }
            });

            if (!isValidTime)
            {
                throw new CustomException.InvalidDataException("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!");
            }

            return isValidTime;
        }
        public static int CheckTimeActiveEvent(TimeSpan time, List<TimeEvent> eventTimes)
        {

            DateTimeOffset dateTime = DateTimeOffset.Now;

            foreach (var eventTime in eventTimes)
            {
                if (eventTime.Date.HasValue)
                {
                    if (eventTime.Date?.Date > dateTime.Date)
                    {
                        return (int)PrefixValueEnum.IsComming;
                    }
                    if (eventTime.Date?.Date == dateTime.Date)
                    {
                        if (eventTime.StartTime > eventTime.EndTime)
                        {
                            if (time <= eventTime.EndTime || eventTime.StartTime >= time)
                            {
                                return (int)PrefixValueEnum.IsGoingOn;
                            };
                        }
                        else
                        {
                            if (time <= eventTime.EndTime && time >= eventTime.StartTime)
                            {
                                return (int)PrefixValueEnum.IsGoingOn;
                            }
                            else if (time < eventTime.StartTime)
                            {
                                return (int)PrefixValueEnum.IsComming;
                            }
                        }
                    }
                }
                else if (eventTime.DayOfWeek.HasValue && eventTime.DayOfWeek.Value == (int)dateTime.DayOfWeek)
                {
                    if (eventTime.StartTime > eventTime.EndTime)
                    {
                        if (time <= eventTime.EndTime || time >= eventTime.StartTime)
                        {
                            return (int)PrefixValueEnum.IsGoingOn;
                        }
                    }
                    else
                    {
                        if (time <= eventTime.EndTime && time >= eventTime.StartTime)
                        {
                            return (int)PrefixValueEnum.IsGoingOn;
                        }
                        else if (time < eventTime.StartTime)
                        {
                            return (int)PrefixValueEnum.IsComming;
                        }
                    }
                }
                else if (eventTime.DayOfWeek.HasValue && eventTime.DayOfWeek.Value > (int)dateTime.DayOfWeek)
                {
                    return (int)PrefixValueEnum.IsComming;
                }
            };
            return (int)PrefixValueEnum.Ended;
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
        public static bool IValidSlot(double requiredHours, TimeSpan StartTime, TimeSpan EndTime)
        {
            double duration = (EndTime < StartTime)
                                ? (EndTime + TimeSpan.FromDays(1) - StartTime).TotalHours
                                : (EndTime - StartTime).TotalHours;
            return duration >= requiredHours;
        }
        public static string GetDayName(int day)
        {
            return day switch
            {
                0 => "Chủ Nhật",
                1 => "Thứ Hai",
                2 => "Thứ Ba",
                3 => "Thứ Tư",
                4 => "Thứ Năm",
                5 => "Thứ Sáu",
                6 => "Thứ Bảy",
                _ => "Không xác định"
            };
        }
        public static int DetermineEventStatus(List<EventTimeResponse> eventTimeResponses,List<TimeEvent> timeEvent,TimeSpan currentTime)
        {
            int? isStillDate = !eventTimeResponses.Any(t => t.Date.HasValue)
                ? null
                : eventTimeResponses
                    .Where(t => t.Date.HasValue)
                    .Select(t => Utils.CheckTimeActiveEvent(currentTime, timeEvent))
                    .FirstOrDefault();

            int? isStillDOW = !eventTimeResponses.Any(t => t.DayOfWeek.HasValue)
                ? null
                : eventTimeResponses
                    .Where(t => t.DayOfWeek.HasValue)
                    .Select(t => Utils.CheckTimeActiveEvent(currentTime, timeEvent))
                    .FirstOrDefault();

            if (isStillDate.HasValue && isStillDate == (int)PrefixValueEnum.IsGoingOn ||
                isStillDOW.HasValue && isStillDOW == (int)PrefixValueEnum.IsGoingOn)
            {
                return (int)PrefixValueEnum.IsGoingOn;
            }

            if (isStillDate.HasValue && isStillDate == (int)PrefixValueEnum.IsComming ||
                isStillDOW.HasValue && isStillDOW == (int)PrefixValueEnum.IsComming)
            {
                return (int)PrefixValueEnum.IsComming;
            }

            return (int)PrefixValueEnum.Ended;
        }
    }
}
