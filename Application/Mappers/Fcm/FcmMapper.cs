using Application.DTOs.Fcm;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.Fcm
{
    public class FcmMapper : Profile
    {
        public FcmMapper()
        {
            CreateMap<CreateNotificationRequest, FcmNotification>();

            CreateMap<FcmNotification, NotificationResponse>()
                .ForMember(dest => dest.BarName, opt => opt.MapFrom(src => src.Bar.BarName))
                .ForMember(dest => dest.IsRead, opt => opt.Ignore())
                .ForMember(dest => dest.ReadAt, opt => opt.Ignore());
        }
    }
}
