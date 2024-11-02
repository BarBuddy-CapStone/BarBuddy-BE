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
        public NotificationMapper()
        {
            CreateMap<NotificationRequest, Notification>().ReverseMap();
            CreateMap<Notification, NotificationResponse>()
                .ForMember(dst => dst.Image, src => src.MapFrom(x =>
                                    !string.IsNullOrEmpty(x.Bar.Images)
                                        ? x.Bar.Images.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                                        : null))
                .ReverseMap();
            CreateMap<NotificationDetail, NotificationResponse>()
                .ForMember(dst => dst.IsRead, src => src.Ignore())
                .ForMember(dst => dst.Title, src => src.MapFrom(x => x.Notification.Title))
                .ForMember(dst => dst.Message, src => src.MapFrom(x => x.Notification.Message))
                .ForMember(dst => dst.Image, src => src.MapFrom(x => !string.IsNullOrEmpty(x.Notification.Bar.Images)
                                        ? x.Notification.Bar.Images.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                                        : null));
        }
    }
}
