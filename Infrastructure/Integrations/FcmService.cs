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

        private async Task SendPushNotification(string deviceToken, FcmNotification notification, Dictionary<string, string>? additionalData = null)
        {
            var data = new Dictionary<string, string>
            {
                { "notificationId", notification.Id.ToString() },
                { "type", notification.Type.ToString() },
                { "deepLink", notification.DeepLink ?? "" },
                { "barId", notification.BarId?.ToString() ?? "" }
            };

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
                    await SendPushNotification(device.DeviceToken, notification, request.Data);
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
                data = request.Data,
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

        public async Task SendBroadcastNotification(string title, string message, Dictionary<string, string> data = null)
        {
            var request = new CreateNotificationRequest
            {
                Title = title,
                Message = message,
                Type = FcmNotificationType.SYSTEM,
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
    }
}
