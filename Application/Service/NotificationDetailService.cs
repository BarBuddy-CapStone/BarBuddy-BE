using Application.DTOs.Notification;
using Application.DTOs.NotificationDetails;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class NotificationDetailService : INotificationDetailService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private IHttpContextAccessor _contextAccessor;
        private IAuthentication _authentication;

        public NotificationDetailService(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, IAuthentication authentication)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _contextAccessor = contextAccessor;
            _authentication = authentication;
        }

        public async Task<bool> CreateNotificationDetail(NotificationDetailRequest request)
        {
            try
            {
                var userId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);

                if (!userId.Equals(request.AccountId))
                {
                    throw new CustomException.InvalidDataException("Bạn không có quyền !");
                }

                var mapper = _mapper.Map<NotificationDetail>(request);
                await _unitOfWork.NotificationDetailRepository.InsertAsync(mapper);
                await Task.Delay(200);
                await _unitOfWork.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new CustomException.InternalServerErrorException(ex.Message, ex);
            }
        }

    }
}
