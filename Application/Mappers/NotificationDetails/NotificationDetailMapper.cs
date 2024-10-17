using Application.DTOs.NotificationDetails;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.NotificationDetails
{
    public class NotificationDetailMapper : Profile
    {
        public NotificationDetailMapper() {
            CreateMap<NotificationDetailRequest, NotificationDetail>().ReverseMap();
            CreateMap<Account, NotificationDetailResponse>()
                .ForMember(dst => dst.Fullname, src => src.MapFrom(x => x.Fullname))
                .ForMember(dst => dst.AccountId, src => src.MapFrom(x => x.AccountId))
                .ReverseMap();
        }
    }
}
