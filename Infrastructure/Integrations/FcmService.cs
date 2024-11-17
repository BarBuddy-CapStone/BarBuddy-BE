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

namespace Infrastructure.Integrations
{
    public class FcmService : IFcmService
    {
        private readonly FirebaseMessaging _firebaseMessaging;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FcmService(FirebaseMessaging firebaseMessaging, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _firebaseMessaging = firebaseMessaging;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task SaveUserDeviceToken(Guid accountId, string deviceToken, string platform)
        {
            var device = new FcmUserDevice
            {
                AccountId = accountId,
                DeviceToken = deviceToken,
                Platform = platform,
                LastLoginAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.FcmUserDeviceRepository.InsertAsync(device);
            await _unitOfWork.SaveAsync();
        }

        public async Task SendNotificationToUser(Guid accountId, string title, string message, Dictionary<string, string> data = null)
        {
            var devices = await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => d.AccountId == accountId && d.IsActive);

            foreach (var device in devices)
            {
                var msg = new Message()
                {
                    Token = device.DeviceToken,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = message
                    },
                    Data = data
                };

                try
                {
                    await _firebaseMessaging.SendAsync(msg);
                }
                catch (FirebaseMessagingException ex)
                {
                    // Token không hợp lệ, deactive device
                    if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                    {
                        device.IsActive = false;
                        await _unitOfWork.SaveAsync();
                    }
                }
            }
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
            var devices = await _unitOfWork.FcmUserDeviceRepository
                .GetAsync(d => accountIds.Contains(d.AccountId) && d.IsActive);

            var messages = devices.Select(d => new Message
            {
                Token = d.DeviceToken,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = message
                },
                Data = data
            }).ToList();

            await _firebaseMessaging.SendAllAsync(messages);
        }
    }
}
