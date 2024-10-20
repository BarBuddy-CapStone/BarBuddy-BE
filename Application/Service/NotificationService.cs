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
                var mapper  = _mapper.Map<Notification>(request);
                
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
                foreach(var notiId in request.NotificationId)
                {
                    var isExist = _unitOfWork.NotificationDetailRepository
                                                .Get(filter: x => x.AccountId.Equals(userId)
                                                                && x.NotificationId.Equals(notiId)
                                                                && x.IsRead == PrefixKeyConstant.FALSE,
                                                            includeProperties: "Notification")
                                                .FirstOrDefault();
                    if (isExist == null)
                    {
                        continue;
                    }

                    isExist.IsRead = PrefixKeyConstant.TRUE;
                    isExist.Notification.UpdatedAt = TimeHelper.ConvertToUtcPlus7(DateTimeOffset.UtcNow);
                    await _unitOfWork.NotificationRepository.UpdateAsync(isExist.Notification);
                    listNoti.Add(isExist);
                }
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();
                var response = _mapper.Map<List<NotificationResponse>>(listNoti);
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

                var getNotiOfCusById = await _unitOfWork.NotificationRepository
                                                    .GetAsync(filter: x => x.NotificationDetails.Any(x => x.AccountId.Equals(accountId))
                                                        , includeProperties: "NotificationDetails.Account");
                var getInfo = getNotiOfCusById?.FirstOrDefault()?.NotificationDetails?.FirstOrDefault()?.Account;
                var response = _mapper.Map<NotificationDetailResponse>(getInfo);
                response.NotificationResponses = _mapper.Map<List<NotificationResponse>>(getNotiOfCusById);

                return response;
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }

        }
    }
}
