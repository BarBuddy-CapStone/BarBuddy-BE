﻿using Application.DTOs.Notification;
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
            CreateMap<NotificationDetail, NotificationResponse>()
                .ForMember(dst => dst.IsRead, src => src.MapFrom(x => x.IsRead))
                .ForMember(dst => dst.Title, src => src.MapFrom(x => x.Notification.Title))
                .ForMember(dst => dst.Message, src => src.MapFrom(x => x.Notification.Message));
        }
    }
}
