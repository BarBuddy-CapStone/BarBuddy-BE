using Application.DTOs.Event;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappers.Events
{
    public class EventMapper : Profile
    {
        public EventMapper()
        {
            CreateMap<EventRequest, Event>().ReverseMap();
        }
    }
}
