using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Fcm;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.SignalR;
using Infrastructure.SignalR;
using Domain.Enums;

namespace Infrastructure.Integrations
{
    public class FcmService : IFcmService
    {
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IConnectionMapping _connectionMapping;

        public FcmService(FirebaseMessaging firebaseMessaging, IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> notificationHub, IConnectionMapping connectionMapping)
        {
            _firebaseMessaging = firebaseMessaging;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationHub = notificationHub;
            _connectionMapping = connectionMapping;
        }

        private async Task SendPushNotification(string deviceToken, FcmNotification notification, Dictionary<string, string>? additionalData = null)
        {
            var data = new Dictionary<string, string>
            {
                { "notificationId", notification.Id.ToString() },
                { "type", notification.Type.ToString() },
                { "deepLink", notification.DeepLink ?? "" },
                { "barId", notification.BarId?.ToString() ?? "" }
            };

            // Merge additional data if provided
            if (additionalData != null)
            {
                foreach (var item in additionalData)
                {
                    data[item.Key] = item.Value;
                }
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

            await _firebaseMessaging.SendAsync(message);
        }

        public async Task SendNotificationToUser(Guid accountId, string title, string message, Dictionary<string, string> data = null)
        {
            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = FcmNotificationType.SYSTEM,
                IsPublic = false,
                Data = data ?? new Dictionary<string, string>()
            };

            await CreateAndSendNotification(request);
        }

        public async Task SendNotificationToTopic(string topic, string title, string message, Dictionary<string, string> data = null)
        {
            var msg = new Message()
            {
                Topic = topic,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = message
                },
                Data = data
            };

            await _firebaseMessaging.SendAsync(msg);
        }

        public async Task SendNotificationToMultipleUsers(List<Guid> accountIds, string title, string message, Dictionary<string, string> data = null)
        {
            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = FcmNotificationType.SYSTEM,
                IsPublic = false,
                Data = data ?? new Dictionary<string, string>()
            };

            await CreateAndSendNotification(request);
        }

        public async Task<Guid> CreateAndSendNotification(CreateNotificationRequest request)
        {
            // 1. Lưu notification vào database
            var notification = _mapper.Map<FcmNotification>(request);
            await _unitOfWork.FcmNotificationRepository.InsertAsync(notification);

            // 2. Gửi push notification qua FCM và tạo notification records
            var devices = await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.IsActive && (request.IsPublic || d.AccountId.HasValue));

            foreach (var device in devices)
            {
                try
                {
                    await SendPushNotification(device.DeviceToken, notification, request.Data);

                    // 3. Tạo notification customer record
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

            // 4. Gửi realtime notification qua SignalR
            var notificationData = new
            {
                id = notification.Id,
                title = notification.Title,
                message = notification.Message,
                type = notification.Type,
                timestamp = notification.CreatedAt,
                data = request.Data,
                isRead = false,
                barId = notification.BarId,
                isPublic = request.IsPublic,
                readAt = (DateTimeOffset?)null
            };

            if (request.IsPublic)
            {
                // Gửi cho tất cả devices
                foreach (var device in devices)
                {
                    var connectionIds = _connectionMapping.GetConnectionIds(device.DeviceToken);
                    if (connectionIds.Any())
                    {
                        await _notificationHub.Clients.Clients(connectionIds)
                            .SendAsync("ReceiveNotification", notificationData);
                        
                        // Cập nhật số lượng thông báo chưa đọc
                        var unreadCount = await GetUnreadCount(device.DeviceToken, device.AccountId);
                        await _notificationHub.Clients.Clients(connectionIds)
                            .SendAsync("ReceiveUnreadCount", unreadCount);
                    }
                }
            }
            else
            {
                // Gửi cho specific users
                foreach (var device in devices.Where(d => d.AccountId.HasValue))
                {
                    await _notificationHub.Clients.Group($"user_{device.AccountId}")
                        .SendAsync("ReceiveNotification", notificationData);
                    
                    // Cập nhật số lượng thông báo chưa đọc
                    var unreadCount = await GetUnreadCount(device.DeviceToken, device.AccountId);
                    await _notificationHub.Clients.Group($"user_{device.AccountId}")
                        .SendAsync("ReceiveUnreadCount", unreadCount);
                }
            }

            await _unitOfWork.SaveAsync();
            return notification.Id;
        }

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

        public async Task SendBroadcastNotification(string title, string message, Dictionary<string, string> data = null)
        {
            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = Domain.Enums.FcmNotificationType.SYSTEM,
                IsPublic = true,
                Data = data
            };

            await CreateAndSendNotification(request);
        }

        public async Task<List<NotificationResponse>> GetNotificationsForUser(Guid accountId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;
            var device = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.AccountId == accountId)).FirstOrDefault();

            if (device == null) return new List<NotificationResponse>();

            var notifications = await _unitOfWork.FcmNotificationRepository
                .GetAsync(
                    n => n.NotificationCustomers.Any(nc =>
                        (nc.CustomerId == accountId) ||
                        (n.IsPublic && nc.DeviceToken == device.DeviceToken)),
                    includeProperties: "Bar,NotificationCustomers"
                );

            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(n =>
                {
                    var response = _mapper.Map<NotificationResponse>(n);
                    var customerNotification = n.NotificationCustomers
                        .FirstOrDefault(nc => nc.CustomerId == accountId || nc.DeviceToken == device.DeviceToken);

                    if (customerNotification != null)
                    {
                        response.IsRead = customerNotification.IsRead;
                        response.ReadAt = customerNotification.ReadAt;
                    }

                    return response;
                })
                .ToList();
        }

        public async Task<List<NotificationResponse>> GetPublicNotifications(string deviceToken, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;

            var notifications = await _unitOfWork.FcmNotificationRepository
                .GetAsync(
                    n => n.IsPublic && n.NotificationCustomers.Any(nc => nc.DeviceToken == deviceToken),
                    includeProperties: "Bar,NotificationCustomers"
                );

            return notifications
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

        public async Task MarkAsRead(Guid notificationId, Guid accountId)
        {
            var device = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.AccountId == accountId && !d.IsGuest)).FirstOrDefault();

            if (device == null) return;

            var notificationCustomer = (await _unitOfWork.FcmNotificationCustomerRepository
                .GetAsync(nc =>
                    nc.NotificationId == notificationId &&
                    nc.CustomerId == accountId)).FirstOrDefault();

            if (notificationCustomer != null && !notificationCustomer.IsRead)
            {
                notificationCustomer.IsRead = true;
                notificationCustomer.ReadAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveAsync();

                await _notificationHub.Clients.User(accountId.ToString())
                    .SendAsync("NotificationRead", new
                    {
                        notificationId = notificationId,
                        readAt = notificationCustomer.ReadAt
                    });
            }
        }

        public async Task MarkAllAsRead(Guid accountId)
        {
            var device = (await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.AccountId == accountId && !d.IsGuest)).FirstOrDefault();

            if (device == null) return;

            var unreadNotifications = await _unitOfWork.FcmNotificationCustomerRepository
                .GetAsync(nc => nc.CustomerId == accountId && !nc.IsRead);

            if (unreadNotifications.Any())
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                }
                await _unitOfWork.SaveAsync();

                await _notificationHub.Clients.User(accountId.ToString())
                    .SendAsync("NotificationsAllRead", new
                    {
                        notificationIds = unreadNotifications.Select(n => n.NotificationId).ToList(),
                        readAt = now,
                        accountId = accountId
                    });
            }
        }

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
                    await _unitOfWork.FcmUserDeviceRepository.UpdateAsync(device);
                    await _unitOfWork.SaveAsync();
                }
                else
                {
                    device.AccountId = null;
                    device.IsGuest = true;
                    await _unitOfWork.FcmUserDeviceRepository.UpdateAsync(device);
                    await _unitOfWork.SaveAsync();
                }
            }
        }

        public async Task MarkAsReadByDeviceToken(Guid notificationId, string deviceToken)
        {
            var notificationCustomer = (await _unitOfWork.FcmNotificationCustomerRepository
                .GetAsync(nc =>
                    nc.NotificationId == notificationId &&
                    nc.DeviceToken == deviceToken)).FirstOrDefault();

            if (notificationCustomer != null && !notificationCustomer.IsRead)
            {
                notificationCustomer.IsRead = true;
                notificationCustomer.ReadAt = DateTimeOffset.UtcNow;
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task MarkAllAsReadByDeviceToken(string deviceToken)
        {
            var unreadNotifications = await _unitOfWork.FcmNotificationCustomerRepository
                .GetAsync(nc => nc.DeviceToken == deviceToken && !nc.IsRead);

            if (unreadNotifications.Any())
            {
                var now = DateTimeOffset.UtcNow;
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                }
                await _unitOfWork.SaveAsync();

                // Gửi SignalR
                var connectionIds = _connectionMapping.GetConnectionIds(deviceToken);
                if (connectionIds.Any())
                {
                    await _notificationHub.Clients
                        .Clients(connectionIds)
                        .SendAsync("NotificationsAllRead", new
                        {
                            notificationIds = unreadNotifications.Select(n => n.NotificationId).ToList(),
                            readAt = now
                        });
                }
            }
        }
    }
}
