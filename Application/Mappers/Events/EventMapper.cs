using Application.DTOs.Event;
using Application.DTOs.Events;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappers.Events
{
    public class EventMapper : Profile
    {
        public EventMapper()
        {
            CreateMap<EventRequest, Event>().ReverseMap();
            CreateMap<Event, EventResponse>()
                .ForMember(dst => dst.BarId, src => src.MapFrom(x => x.Bar.BarId))
                .ReverseMap();
            CreateMap<UpdateEventRequest, Event>().ReverseMap();
        }
    }
}
