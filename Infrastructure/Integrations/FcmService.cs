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

            if (request.IsPublic)
            {
                foreach (var device in devices)
                {
                    if (device.AccountId.HasValue)
                    {
                        await _notificationHub.Clients.Group($"user_{device.AccountId}")
                            .SendAsync("ReceiveNotification", notificationData);
                    }
                    else
                    {
                        var connectionIds = _connectionMapping.GetConnectionIds(device.DeviceToken);
                        if (connectionIds.Any())
                        {
                            await _notificationHub.Clients.Clients(connectionIds)
                                .SendAsync("ReceiveNotification", notificationData);
                        }
                    }
                }
            }
            else
            {
                var userDevices = devices.Where(d => d.AccountId.HasValue);
                foreach (var device in userDevices)
                {
                    await _notificationHub.Clients.Group($"user_{device.AccountId}")
                        .SendAsync("ReceiveNotification", notificationData);
                }
            }

            await _unitOfWork.SaveAsync();
            return notification.Id;
        }

        // Send notification to user    
        public async Task SendNotificationToUser(Guid accountId, string title, string message, Dictionary<string, string> data = null)
        {
            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = FcmNotificationType.SYSTEM,
                IsPublic = false
            };

            await CreateAndSendNotification(request);
        }

        // Send broadcast notification
        public async Task SendBroadcastNotification(string title, string message, Dictionary<string, string> data = null)
        {
            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = FcmNotificationType.SYSTEM,
                IsPublic = true
            };

            await CreateAndSendNotification(request);
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
    }
}
