using Application.DTOs.Notification;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.Notifications
{
    public class NotificationMapper : Profile
    {
        public NotificationMapper() {
            CreateMap<NotificationRequest, Notification>().ReverseMap();
            CreateMap<Notification, NotificationResponse>().ReverseMap();
        }
    }
}
