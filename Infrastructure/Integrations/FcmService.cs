using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Fcm;
using Microsoft.AspNetCore.SignalR;
using Infrastructure.SignalR;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Integrations
{
    public class FcmService : IFcmService
    {
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IConnectionMapping _connectionMapping;

        public FcmService(FirebaseMessaging firebaseMessaging, IUnitOfWork unitOfWork, IMapper mapper, 
            IHubContext<NotificationHub> notificationHub, IConnectionMapping connectionMapping)
        {
            _firebaseMessaging = firebaseMessaging;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationHub = notificationHub;
            _connectionMapping = connectionMapping;
        }

        public async Task<Guid> CreateAndSendNotification(CreateNotificationRequest request)
        {
            var notification = _mapper.Map<FcmNotification>(request);
            await _unitOfWork.FcmNotificationRepository.InsertAsync(notification);

            // Xác định danh sách account sẽ nhận thông báo
            var accountIds = new List<Guid>();
            if (request.IsPublic)
            {
                // Nếu là public, lấy tất cả account
                var accounts = await _unitOfWork.AccountRepository.GetAsync();
                accountIds.AddRange(accounts.Select(a => a.AccountId));
            }
            else if (request.SpecificAccountIds != null && request.SpecificAccountIds.Any())
            {
                // Nếu có danh sách account cụ thể
                accountIds.AddRange(request.SpecificAccountIds);
            }

            // Tạo NotificationAccount cho từng account
            foreach (var accountId in accountIds)
            {
                await _unitOfWork.NotificationAccountRepository.InsertAsync(new NotificationAccount
                {
                    NotificationId = notification.Id,
                    AccountId = accountId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            await _unitOfWork.SaveAsync();

            // Gửi thông báo qua SignalR
            var notificationData = new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type,
                timestamp = notification.CreatedAt,
                mobileDeepLink = notification.MobileDeepLink,
                webDeepLink = notification.WebDeepLink,
                imageUrl = notification.ImageUrl,
                isRead = false,
                barId = notification.BarId,
                isPublic = request.IsPublic,
                readAt = (DateTimeOffset?)null
            };

            foreach (var accountId in accountIds)
            {
                await _notificationHub.Clients.Group($"user_{accountId}")
                    .SendAsync("ReceiveNotification", notificationData);

                // Gửi số lượng thông báo chưa đọc mới
                var unreadCount = await GetUnreadNotificationCount(accountId);
                await _notificationHub.Clients.Group($"user_{accountId}")
                    .SendAsync("ReceiveUnreadCount", unreadCount);
            }

            // Gửi push notification cho các device
            var devices = await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.IsActive && 
                    (request.IsPublic || 
                     (request.SpecificAccountIds != null && request.SpecificAccountIds.Contains(d.AccountId.Value))));

            foreach (var device in devices)
            {
                try
                {
                    await SendPushNotification(device.DeviceToken, notification);
                }
                catch (Exception ex)
                {
                    // Log error but continue sending to other devices
                    Console.WriteLine($"Error sending push notification to device {device.DeviceToken}: {ex.Message}");
                }
            }

            return notification.Id;
        }

        public async Task<List<NotificationResponse>> GetNotifications(Guid accountId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var notifications = await _unitOfWork.FcmNotificationRepository
                .GetAsync(
                    n => n.NotificationAccounts.Any(na => na.AccountId == accountId),
                    includeProperties: "Bar,NotificationAccounts"
                );

            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(n =>
                {
                    var response = _mapper.Map<NotificationResponse>(n);
                    var accountNotification = n.NotificationAccounts
                        .FirstOrDefault(na => na.AccountId == accountId);

                    if (accountNotification != null)
                    {
                        response.IsRead = accountNotification.IsRead;
                        response.ReadAt = accountNotification.ReadAt;
                    }

                    return response;
                })
                .ToList();
        }

        public async Task<int> GetUnreadNotificationCount(Guid accountId)
        {
            var notifications = await _unitOfWork.FcmNotificationRepository
                .GetAsync(
                    n => n.NotificationAccounts.Any(na => 
                        na.AccountId == accountId && !na.IsRead),
                    includeProperties: "NotificationAccounts"
                );

            return notifications.Count();
        }

        public async Task MarkAsRead(Guid accountId, Guid notificationId)
        {
            var notificationAccount = (await _unitOfWork.NotificationAccountRepository
                .GetAsync(na => 
                    na.NotificationId == notificationId && 
                    na.AccountId == accountId))
                .FirstOrDefault();

            if (notificationAccount != null && !notificationAccount.IsRead)
            {
                notificationAccount.IsRead = true;
                notificationAccount.ReadAt = DateTimeOffset.UtcNow;
                await _unitOfWork.NotificationAccountRepository.UpdateAsync(notificationAccount);
                await _unitOfWork.SaveAsync();

                // Lấy thông tin notification để gửi qua SignalR
                var notification = (await _unitOfWork.FcmNotificationRepository
                    .GetAsync(n => n.Id == notificationId, includeProperties: "Bar"))
                    .FirstOrDefault();

                if (notification != null)
                {
                    var notificationData = new
                    {
                        id = notification.Id,
                        title = notification.Title,
                        message = notification.Message,
                        type = notification.Type,
                        timestamp = notification.CreatedAt,
                        mobileDeepLink = notification.MobileDeepLink,
                        webDeepLink = notification.WebDeepLink,
                        imageUrl = notification.ImageUrl,
                        isRead = true,
                        barId = notification.BarId,
                        isPublic = notification.IsPublic,
                        readAt = notificationAccount.ReadAt
                    };

                    await _notificationHub.Clients.Group($"user_{accountId}")
                        .SendAsync("ReceiveNotification", notificationData);

                    // Gửi số lượng thông báo chưa đọc mới
                    var unreadCount = await GetUnreadNotificationCount(accountId);
                    await _notificationHub.Clients.Group($"user_{accountId}")
                        .SendAsync("ReceiveUnreadCount", unreadCount);
                }
            }
        }

        public async Task MarkAllAsRead(Guid accountId)
        {
            var notificationAccounts = await _unitOfWork.NotificationAccountRepository
                .GetAsync(na => !na.IsRead && na.AccountId == accountId);

            foreach (var na in notificationAccounts)
            {
                na.IsRead = true;
                na.ReadAt = DateTimeOffset.UtcNow;
                await _unitOfWork.NotificationAccountRepository.UpdateAsync(na);
            }

            await _unitOfWork.SaveAsync();

            // Lấy danh sách notifications để gửi qua SignalR
            var notifications = await _unitOfWork.FcmNotificationRepository
                .GetAsync(n => notificationAccounts.Select(na => na.NotificationId).Contains(n.Id), 
                    includeProperties: "Bar");

            foreach (var notification in notifications)
            {
                var notificationData = new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    timestamp = notification.CreatedAt,
                    mobileDeepLink = notification.MobileDeepLink,
                    webDeepLink = notification.WebDeepLink,
                    imageUrl = notification.ImageUrl,
                    isRead = true,
                    barId = notification.BarId,
                    isPublic = notification.IsPublic,
                    readAt = DateTimeOffset.UtcNow
                };

                await _notificationHub.Clients.Group($"user_{accountId}")
                    .SendAsync("ReceiveNotification", notificationData);
            }

            // Gửi số lượng thông báo chưa đọc mới (sẽ là 0)
            await _notificationHub.Clients.Group($"user_{accountId}")
                .SendAsync("ReceiveUnreadCount", 0);
        }

        public async Task SaveDeviceToken(string deviceToken, string platform)
        {
            // Validate platform
            platform = platform.ToLower();
            if (!IsValidPlatform(platform))
            {
                throw new ArgumentException("Không hợp lệ. Platform phải là 'web', 'ios', hoặc 'android'");
            }

            var existingDevice = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.DeviceToken == deviceToken))
                .FirstOrDefault();

            if (existingDevice == null)
            {
                var device = new FcmUserDevice
                {
                    DeviceToken = deviceToken,
                    Platform = platform,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };
                await _unitOfWork.FcmUserDeviceRepository.InsertAsync(device);
                await _unitOfWork.SaveAsync();
            }
            else if (!existingDevice.IsActive || existingDevice.Platform != platform)
            {
                existingDevice.IsActive = true;
                existingDevice.Platform = platform;
                await _unitOfWork.FcmUserDeviceRepository.UpdateAsync(existingDevice);
                await _unitOfWork.SaveAsync();
            }
        }

        private bool IsValidPlatform(string platform)
        {
            return platform == "web" || platform == "ios" || platform == "android";
        }

        public async Task LinkDeviceTokenToAccount(Guid accountId, string deviceToken)
        {
            var device = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.DeviceToken == deviceToken))
                .FirstOrDefault();

            if (device != null)
            {
                device.AccountId = accountId;
                await _unitOfWork.FcmUserDeviceRepository.UpdateAsync(device);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task UnlinkDeviceToken(string deviceToken)
        {
            var device = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.DeviceToken == deviceToken))
                .FirstOrDefault();

            if (device != null)
            {
                device.AccountId = null;
                await _unitOfWork.FcmUserDeviceRepository.UpdateAsync(device);
                await _unitOfWork.SaveAsync();
            }
        }

        private async Task SendPushNotification(string deviceToken, FcmNotification notification)
        {
            // Lấy device info để biết là web hay mobile
            var device = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.DeviceToken == deviceToken))
                .FirstOrDefault();

            if (device == null) return;

            var data = new Dictionary<string, string>
            {
                { "notificationId", notification.Id.ToString() },
                { "type", notification.Type.ToString() },
                { "barId", notification.BarId?.ToString() ?? "" }
            };

            // Thêm deeplink phù hợp theo platform
            if (device.Platform.ToLower() == "web")
            {
                data.Add("deepLink", notification.WebDeepLink ?? "");
            }
            else // mobile platforms (ios, android)
            {
                data.Add("deepLink", notification.MobileDeepLink ?? "");
            }

            var message = new Message()
            {
                Token = deviceToken,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = notification.Title,
                    Body = notification.Message,
                    ImageUrl = notification.ImageUrl
                },
                Data = data
            };

            try
            {
                await _firebaseMessaging.SendAsync(message);
            }
            catch (FirebaseMessagingException ex)
            {
                if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                {
                    device.IsActive = false;
                    await _unitOfWork.FcmUserDeviceRepository.UpdateAsync(device);
                    await _unitOfWork.SaveAsync();
                }
                throw;
            }
        }

        public async Task SendPushNotificationToAllDevices(CreateNotificationRequest request)
        {
            try
            {
                // Lấy tất cả active device tokens
                var activeDevices = await _unitOfWork.FcmUserDeviceRepository
                    .GetAsync(d => d.IsActive);

                if (!activeDevices.Any()) return;

                // Tạo notification object để lưu thông tin
                var notification = _mapper.Map<FcmNotification>(request);
                await _unitOfWork.FcmNotificationRepository.InsertAsync(notification);
                await _unitOfWork.SaveAsync();

                // Tạo message template
                var messageTemplate = new Message
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = request.Title,
                        Body = request.Message,
                        ImageUrl = request.ImageUrl
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "notificationId", notification.Id.ToString() },
                        { "type", notification.Type.ToString() },
                        { "barId", notification.BarId?.ToString() ?? "" }
                    }
                };

                // Gửi theo batch để tránh quá tải
                const int batchSize = 500; // Firebase giới hạn 500 tokens/request
                var deviceBatches = activeDevices
                    .Select(d => d.DeviceToken)
                    .Distinct() // Loại bỏ token trùng lặp
                    .Select((token, index) => new { token, index })
                    .GroupBy(x => x.index / batchSize)
                    .Select(g => g.Select(x => x.token).ToList());

                foreach (var batch in deviceBatches)
                {
                    try
                    {
                        var messages = batch.Select(token =>
                        {
                            var deviceInfo = activeDevices.First(d => d.DeviceToken == token);
                            var messageData = new Dictionary<string, string>(messageTemplate.Data); // Tạo bản sao mới

                            // Thêm deepLink phù hợp theo platform
                            if (deviceInfo.Platform.ToLower() == "web")
                            {
                                messageData.Add("deepLink", notification.WebDeepLink ?? "");
                            }
                            else
                            {
                                messageData.Add("deepLink", notification.MobileDeepLink ?? "");
                            }

                            return new Message
                            {
                                Token = token,
                                Notification = messageTemplate.Notification,
                                Data = messageData
                            };
                        }).ToList();

                        var response = await _firebaseMessaging.SendAllAsync(messages);

                        // Xử lý các token không hợp lệ
                        if (response.FailureCount > 0)
                        {
                            var failedTokens = new List<string>();
                            for (var i = 0; i < response.Responses.Count; i++)
                            {
                                if (!response.Responses[i].IsSuccess &&
                                    response.Responses[i].Exception?.MessagingErrorCode == MessagingErrorCode.Unregistered)
                                {
                                    failedTokens.Add(batch[i]);
                                }
                            }

                            // Deactivate các token không hợp lệ
                            if (failedTokens.Any())
                            {
                                var devicesToUpdate = await _unitOfWork.FcmUserDeviceRepository
                                    .GetAsync(d => failedTokens.Contains(d.DeviceToken));

                                foreach (var device in devicesToUpdate)
                                {
                                    device.IsActive = false;
                                    await _unitOfWork.FcmUserDeviceRepository.UpdateAsync(device);
                                }
                                await _unitOfWork.SaveAsync();
                            }
                        }

                        // Log kết quả
                        Console.WriteLine($"Batch sent: Success={response.SuccessCount}, Failure={response.FailureCount}");
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nhưng vẫn tiếp tục gửi các batch khác
                        Console.WriteLine($"Error sending batch: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendPushNotificationToAllDevices: {ex.Message}");
                throw;
            }
        }
    }
}
