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

        private async Task SendPushNotification(string deviceToken, FcmNotification notification)
        {
            var data = new Dictionary<string, string>
            {
                { "notificationId", notification.Id.ToString() },
                { "type", notification.Type.ToString() },
                { "deepLink", notification.DeepLink ?? "" },
                { "barId", notification.BarId?.ToString() ?? "" }
            };

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

            await _firebaseMessaging.SendAsync(message);
        }

        // Save guest device token
        public async Task SaveGuestDeviceToken(string deviceToken, string platform)
        {
            var existingDevice = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.DeviceToken == deviceToken)).FirstOrDefault();

            if (existingDevice == null)
            {
                var device = new FcmUserDevice
                {
                    AccountId = null,
                    DeviceToken = deviceToken,
                    Platform = platform,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsGuest = true
                };
                await _unitOfWork.FcmUserDeviceRepository.InsertAsync(device);
                await _unitOfWork.SaveAsync();
            }
        }

        // Update device token for user
        public async Task UpdateDeviceTokenForUser(Guid accountId, UpdateDeviceTokenRequest request)
        {
            var device = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.DeviceToken == request.DeviceToken)).FirstOrDefault();

            if (device != null)
            {
                if (request.IsLoginOrLogout)
                {
                    device.AccountId = accountId;
                    device.IsGuest = false;
                }
                else
                {
                    device.AccountId = null;
                    device.IsGuest = true;
                }
                await _unitOfWork.FcmUserDeviceRepository.UpdateAsync(device);
                await _unitOfWork.SaveAsync();
            }
        }

        // Create and send notification 
        public async Task<Guid> CreateAndSendNotification(CreateNotificationRequest request)
        {
            var notification = _mapper.Map<FcmNotification>(request);
            await _unitOfWork.FcmNotificationRepository.InsertAsync(notification);

            var devices = await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.IsActive && (request.IsPublic || d.AccountId.HasValue));

            foreach (var device in devices)
            {
                try
                {
                    await SendPushNotification(device.DeviceToken, notification);
                    await _unitOfWork.FcmNotificationCustomerRepository.InsertAsync(
                        new FcmNotificationCustomer
                        {
                            NotificationId = notification.Id,
                            CustomerId = device.AccountId,
                            DeviceToken = device.DeviceToken,
                            CreatedAt = DateTimeOffset.UtcNow
                        }
                    );
                }
                catch (FirebaseMessagingException ex)
                {
                    if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                    {
                        device.IsActive = false;
                    }
                }
            }

            // Lưu thay đổi vào database TRƯỚC KHI gửi SignalR
            await _unitOfWork.SaveAsync();

            // Tạo notification data để gửi qua SignalR
            var notificationData = new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type,
                timestamp = notification.CreatedAt,
                deepLink = notification.DeepLink,
                imageUrl = notification.ImageUrl,
                isRead = false,
                barId = notification.BarId,
                isPublic = request.IsPublic,
                readAt = (DateTimeOffset?)null
            };

            // Lấy tất cả device đang kết nối
            var connections = _connectionMapping.GetAllConnections();
            
            foreach (var connection in connections)
            {
                var deviceToken = connection.Value.DeviceToken;
                var accountId = connection.Value.AccountId;

                // Nếu là thông báo public hoặc user đã đăng nhập
                if (request.IsPublic || !string.IsNullOrEmpty(accountId))
                {
                    // 1. Gửi notification
                    if (!string.IsNullOrEmpty(accountId))
                    {
                        await _notificationHub.Clients.Group($"user_{accountId}")
                            .SendAsync("ReceiveNotification", notificationData);
                    }
                    else if (request.IsPublic)
                    {
                        var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                        if (connectionIds.Any())
                        {
                            await _notificationHub.Clients.Clients(connectionIds)
                                .SendAsync("ReceiveNotification", notificationData);
                        }
                    }

                    // 2. Gửi unread count mới (sau khi đã lưu vào database)
                    var unreadCount = await GetUnreadNotificationCount(
                        deviceToken,
                        !string.IsNullOrEmpty(accountId) ? Guid.Parse(accountId) : null);

                    if (!string.IsNullOrEmpty(accountId))
                    {
                        await _notificationHub.Clients.Group($"user_{accountId}")
                            .SendAsync("ReceiveUnreadCount", unreadCount);
                    }
                    else
                    {
                        var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                        if (connectionIds.Any())
                        {
                            await _notificationHub.Clients.Clients(connectionIds)
                                .SendAsync("ReceiveUnreadCount", unreadCount);
                        }
                    }
                }
            }

            return notification.Id;
        }

        // Get notifications for user
        public async Task<List<NotificationResponse>> GetNotifications(string deviceToken, Guid? accountId = null, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            // Nếu người dùng chưa đăng nhập (chỉ lấy thông báo public theo deviceToken)
            if (!accountId.HasValue)
            {
                var publicNotifications = await _unitOfWork.FcmNotificationRepository
                    .GetAsync(
                        n => n.IsPublic && n.NotificationCustomers.Any(nc => nc.DeviceToken == deviceToken),
                        includeProperties: "Bar,NotificationCustomers"
                    );

                return publicNotifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(n =>
                    {
                        var response = _mapper.Map<NotificationResponse>(n);
                        var customerNotification = n.NotificationCustomers
                            .FirstOrDefault(nc => nc.DeviceToken == deviceToken);

                        if (customerNotification != null)
                        {
                            response.IsRead = customerNotification.IsRead;
                            response.ReadAt = customerNotification.ReadAt;
                        }

                        return response;
                    })
                    .ToList();
            }

            // Nếu người dùng đã đăng nhập (lấy cả thông báo public và private)
            var allNotifications = await _unitOfWork.FcmNotificationRepository
                .GetAsync(
                    n => (n.IsPublic && n.NotificationCustomers.Any(nc => nc.DeviceToken == deviceToken)) || 
                         n.NotificationCustomers.Any(nc => nc.CustomerId == accountId),
                    includeProperties: "Bar,NotificationCustomers"
                );

            // Loại bỏ các thông báo trùng lặp bằng cách group theo Id
            return allNotifications
                .GroupBy(n => n.Id)
                .Select(g => g.First())
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(n =>
                {
                    var response = _mapper.Map<NotificationResponse>(n);
                    var customerNotification = n.NotificationCustomers
                        .FirstOrDefault(nc => nc.CustomerId == accountId || nc.DeviceToken == deviceToken);

                    if (customerNotification != null)
                    {
                        response.IsRead = customerNotification.IsRead;
                        response.ReadAt = customerNotification.ReadAt;
                    }

                    return response;
                })
                .ToList();
        }

        public async Task<int> GetUnreadNotificationCount(string deviceToken, Guid? accountId = null)
        {
            try
            {
                // Nếu người dùng chưa đăng nhập (chỉ lấy thông báo public theo deviceToken)
                if (!accountId.HasValue)
                {
                    var publicNotifications = await _unitOfWork.FcmNotificationRepository
                        .GetAsync(
                            n => n.IsPublic && n.NotificationCustomers.Any(nc => nc.DeviceToken == deviceToken),
                            includeProperties: "NotificationCustomers"
                        );

                    return publicNotifications.Count(n => 
                    {
                        var customerNotification = n.NotificationCustomers
                            .FirstOrDefault(nc => nc.DeviceToken == deviceToken);
                        return customerNotification == null || !customerNotification.IsRead;
                    });
                }

                // Nếu người dùng đã đăng nhập (lấy cả thông báo public và private)
                var allNotifications = await _unitOfWork.FcmNotificationRepository
                    .GetAsync(
                        n => (n.IsPublic && n.NotificationCustomers.Any(nc => nc.DeviceToken == deviceToken)) || 
                             n.NotificationCustomers.Any(nc => nc.CustomerId == accountId),
                        includeProperties: "NotificationCustomers"
                    );

                // Đếm số lượng thông báo chưa đọc (không cần group)
                return allNotifications.Count(n => 
                {
                    var customerNotification = n.NotificationCustomers
                        .FirstOrDefault(nc => nc.CustomerId == accountId || nc.DeviceToken == deviceToken);
                    return customerNotification == null || !customerNotification.IsRead;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnreadNotificationCount: {ex.Message}");
                throw;
            }
        }

        public async Task<Guid> CreateAndSendNotificationToCustomer(CreateNotificationRequest request, Guid customerId)
        {
            var notification = _mapper.Map<FcmNotification>(request);
            await _unitOfWork.FcmNotificationRepository.InsertAsync(notification);

            // Lấy device của customer cụ thể
            var customerDevices = await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.IsActive && d.AccountId == customerId);

            foreach (var device in customerDevices)
            {
                try
                {
                    await SendPushNotification(device.DeviceToken, notification);
                    await _unitOfWork.FcmNotificationCustomerRepository.InsertAsync(
                        new FcmNotificationCustomer
                        {
                            NotificationId = notification.Id,
                            CustomerId = customerId,
                            DeviceToken = device.DeviceToken,
                            CreatedAt = DateTimeOffset.UtcNow
                        }
                    );
                }
                catch (FirebaseMessagingException ex)
                {
                    if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                    {
                        device.IsActive = false;
                    }
                }
            }

            // Lưu thay đổi vào database TRƯỚC KHI gửi SignalR
            await _unitOfWork.SaveAsync();

            // Tạo notification data để gửi qua SignalR
            var notificationData = new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type,
                timestamp = notification.CreatedAt,
                deepLink = notification.DeepLink,
                imageUrl = notification.ImageUrl,
                isRead = false,
                barId = notification.BarId,
                isPublic = false,
                readAt = (DateTimeOffset?)null
            };

            // Lấy tất cả device đang kết nối của customer này
            var connections = _connectionMapping.GetAllConnections()
                .Where(c => c.Value.AccountId == customerId.ToString());
            
            foreach (var connection in connections)
            {
                var deviceToken = connection.Value.DeviceToken;
                var accountId = connection.Value.AccountId;

                // 1. Gửi notification
                await _notificationHub.Clients.Group($"user_{accountId}")
                    .SendAsync("ReceiveNotification", notificationData);

                // 2. Gửi unread count mới
                var unreadCount = await GetUnreadNotificationCount(
                    deviceToken,
                    customerId);

                await _notificationHub.Clients.Group($"user_{accountId}")
                    .SendAsync("ReceiveUnreadCount", unreadCount);
            }

            return notification.Id;
        }

        public async Task MarkAsRead(string deviceToken, Guid notificationId, Guid? accountId = null)
        {
            var notificationCustomer = (await _unitOfWork.FcmNotificationCustomerRepository
                .GetAsync(nc => 
                    nc.NotificationId == notificationId && 
                    ((accountId.HasValue && nc.CustomerId == accountId) || 
                     (!accountId.HasValue && nc.DeviceToken == deviceToken))))
                .FirstOrDefault();

            if (notificationCustomer != null && !notificationCustomer.IsRead)
            {
                notificationCustomer.IsRead = true;
                notificationCustomer.ReadAt = DateTimeOffset.UtcNow;
                await _unitOfWork.FcmNotificationCustomerRepository.UpdateAsync(notificationCustomer);
                await _unitOfWork.SaveAsync();

                // Lấy thông tin notification để gửi qua SignalR
                var notification = (await _unitOfWork.FcmNotificationRepository
                    .GetAsync(n => n.Id == notificationId, includeProperties: "Bar")).FirstOrDefault();

                if (notification != null)
                {
                    var notificationData = new
                    {
                        id = notification.Id,
                        title = notification.Title,
                        message = notification.Message,
                        type = notification.Type,
                        timestamp = notification.CreatedAt,
                        deepLink = notification.DeepLink,
                        imageUrl = notification.ImageUrl,
                        isRead = true,
                        barId = notification.BarId,
                        isPublic = notification.IsPublic,
                        readAt = notificationCustomer.ReadAt
                    };

                    // Gửi notification đã cập nhật
                    if (!string.IsNullOrEmpty(accountId?.ToString()))
                    {
                        await _notificationHub.Clients.Group($"user_{accountId}")
                            .SendAsync("ReceiveNotification", notificationData);
                    }
                    else
                    {
                        var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                        if (connectionIds.Any())
                        {
                            await _notificationHub.Clients.Clients(connectionIds)
                                .SendAsync("ReceiveNotification", notificationData);
                        }
                    }

                    // Gửi số lượng thông báo chưa đọc mới
                    var unreadCount = await GetUnreadNotificationCount(deviceToken, accountId);
                    if (!string.IsNullOrEmpty(accountId?.ToString()))
                    {
                        await _notificationHub.Clients.Group($"user_{accountId}")
                            .SendAsync("ReceiveUnreadCount", unreadCount);
                    }
                    else
                    {
                        var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                        if (connectionIds.Any())
                        {
                            await _notificationHub.Clients.Clients(connectionIds)
                                .SendAsync("ReceiveUnreadCount", unreadCount);
                        }
                    }
                }
            }
        }

        public async Task MarkAllAsRead(string deviceToken, Guid? accountId = null)
        {
            var notificationCustomers = await _unitOfWork.FcmNotificationCustomerRepository
                .GetAsync(nc => 
                    !nc.IsRead && 
                    ((accountId.HasValue && nc.CustomerId == accountId) || 
                     (!accountId.HasValue && nc.DeviceToken == deviceToken)));

            foreach (var nc in notificationCustomers)
            {
                nc.IsRead = true;
                nc.ReadAt = DateTimeOffset.UtcNow;
                await _unitOfWork.FcmNotificationCustomerRepository.UpdateAsync(nc);
            }

            await _unitOfWork.SaveAsync();

            // Lấy danh sách notifications từ database thay vì dùng GetNotifications
            var notificationIds = notificationCustomers.Select(nc => nc.NotificationId).ToList();
            var notifications = await _unitOfWork.FcmNotificationRepository
                .GetAsync(n => notificationIds.Contains(n.Id), includeProperties: "Bar");
            
            foreach (var notification in notifications)
            {
                var notificationData = new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    timestamp = notification.CreatedAt,
                    deepLink = notification.DeepLink,
                    imageUrl = notification.ImageUrl,
                    isRead = true,
                    barId = notification.BarId,
                    isPublic = notification.IsPublic,
                    readAt = DateTimeOffset.UtcNow
                };

                // Gửi notification đã cập nhật
                if (!string.IsNullOrEmpty(accountId?.ToString()))
                {
                    await _notificationHub.Clients.Group($"user_{accountId}")
                        .SendAsync("ReceiveNotification", notificationData);
                }
                else
                {
                    var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                    if (connectionIds.Any())
                    {
                        await _notificationHub.Clients.Clients(connectionIds)
                            .SendAsync("ReceiveNotification", notificationData);
                    }
                }
            }

            // Gửi số lượng thông báo chưa đọc mới (sẽ là 0)
            if (!string.IsNullOrEmpty(accountId?.ToString()))
            {
                await _notificationHub.Clients.Group($"user_{accountId}")
                    .SendAsync("ReceiveUnreadCount", 0);
            }
            else
            {
                var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                if (connectionIds.Any())
                {
                    await _notificationHub.Clients.Clients(connectionIds)
                        .SendAsync("ReceiveUnreadCount", 0);
                }
            }
        }
    }
}
