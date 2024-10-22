using Application.DTOs.Events.EventTime;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappers.Events.EventTimes
{
    public class EventTimeMapper : Profile
    {
        public EventTimeMapper() {
            CreateMap<EventTimeRequest, TimeEvent>().ReverseMap();
            CreateMap<TimeEvent, EventTimeResponse>().ReverseMap();
        }
    }
}
