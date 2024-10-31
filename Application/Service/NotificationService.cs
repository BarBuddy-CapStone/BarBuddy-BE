using Application.DTOs.Notification;
using Application.DTOs.NotificationDetails;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Domain.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class NotificationService : INotificationService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private IAuthentication _authentication;
        private IHttpContextAccessor _contextAccessor;
        private INotificationDetailService _notificationDetailService;
        public NotificationService(IMapper mapper, IUnitOfWork unitOfWork,
                                    IAuthentication authentication, IHttpContextAccessor contextAccessor,
                                    INotificationDetailService notificationDetailService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _authentication = authentication;
            _contextAccessor = contextAccessor;
            _notificationDetailService = notificationDetailService;
        }

        public async Task<NotificationResponse> CreateNotification(NotificationRequest request)
        {
            var userId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);

            try
            {
                var mapper = _mapper.Map<Notification>(request);

                mapper.UpdatedAt = mapper.CreatedAt;

                var notiDetailMapper = new NotificationDetailRequest
                {
                    AccountId = userId,
                    NotificationId = mapper.NotificationId
                };

                await _unitOfWork.NotificationRepository.InsertAsync(mapper);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();
                await _notificationDetailService.CreateNotificationDetail(notiDetailMapper);
                var response = _mapper.Map<NotificationResponse>(mapper);
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<NotificationResponse>> UpdateIsReadNoti(UpdateNotiRequest request)
        {
            try
            {
                var listNoti = new List<NotificationDetail>();
                var userId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                _ = userId.Equals(request.AccountId) ? true : throw new CustomException.InvalidDataException("Bạn không có quyền!");

                var isExist = _unitOfWork.NotificationDetailRepository
                                            .Get(filter: x => x.AccountId.Equals(userId)
                                                            && x.IsRead == PrefixKeyConstant.FALSE,
                                                        includeProperties: "Notification.Bar")
                                            .ToList();
                if (isExist.Any())
                {

                    isExist.ForEach(x =>
                    {
                        x.IsRead = PrefixKeyConstant.TRUE;
                        x.Notification.UpdatedAt = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.UtcNow);
                    });
                    foreach (var noti in isExist)
                    {
                        await _unitOfWork.NotificationRepository.UpdateAsync(noti.Notification);
                        await Task.Delay(10);
                        await _unitOfWork.NotificationDetailRepository.UpdateAsync(noti);
                    }
                }
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();
                var response = _mapper.Map<List<NotificationResponse>>(isExist);
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<NotificationDetailResponse> GetAllNotiOfCus(Guid accountId)
        {

            try
            {
                var userId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);

                if (!userId.Equals(accountId))
                {
                    throw new CustomException.InvalidDataException("Bạn không có quyền !");
                }
                var response = new NotificationDetailResponse();
                var getNotiOfCusById = await _unitOfWork.NotificationRepository
                                                    .GetAsync(filter: x => x.NotificationDetails.Any(x => x.AccountId.Equals(accountId))
                                                        , includeProperties: "NotificationDetails.Account,Bar");
                if (!getNotiOfCusById.IsNullOrEmpty())
                {
                    var getInfo = getNotiOfCusById?.FirstOrDefault()?.NotificationDetails?.FirstOrDefault()?.Account;
                    response = _mapper.Map<NotificationDetailResponse>(getInfo);
                    response.NotificationResponses = _mapper.Map<List<NotificationResponse>>(getNotiOfCusById);
                }
                response.AccountId = accountId;
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }

        }

        public async Task<NotificationResponse> CreateNotificationAllCustomer(Guid accountId, NotificationRequest request)
        {
            try
            {
                var mapper = _mapper.Map<Notification>(request);

                mapper.UpdatedAt = mapper.CreatedAt;

                var notiDetailMapper = new NotificationDetailRequest
                {
                    AccountId = accountId,
                    NotificationId = mapper.NotificationId
                };

                await _unitOfWork.NotificationRepository.InsertAsync(mapper);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();
                await _notificationDetailService.CreateNotificationDetailJob(notiDetailMapper);
                var response = _mapper.Map<NotificationResponse>(mapper);
                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
