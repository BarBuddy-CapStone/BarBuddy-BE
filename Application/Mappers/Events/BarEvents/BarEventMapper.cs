﻿using Application.DTOs.Event;
using Application.DTOs.Events.EventBar;
using AutoMapper;
using Domain.Entities;
namespace Application.Mappers.Events.BarEvents
{
    public class BarEventMapper : Profile
    {
        public BarEventMapper()
        {
            CreateMap<EventRequest, BarEventRequest>().ReverseMap();
            CreateMap<BarEventRequest, BarEvent>().ReverseMap();
            CreateMap<BarEvent, BarEventResponse>()
                .ForMember(dst => dst.BarName, src => src.MapFrom(x => x.Bar.BarName))
                .ForMember(dst => dst.StartTime, src => src.MapFrom(x => x.Bar.StartTime))
                .ForMember(dst => dst.EndTime, src => src.MapFrom(x => x.Bar.EndTime))
                .ReverseMap();
        }
    }
}