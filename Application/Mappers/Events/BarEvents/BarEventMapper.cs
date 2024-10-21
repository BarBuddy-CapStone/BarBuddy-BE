using Application.DTOs.Event;
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
        }
    }
}
