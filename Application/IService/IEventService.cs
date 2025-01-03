﻿using Application.DTOs.Event;
using Application.DTOs.Events;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IEventService
    {
        Task CreateEvent(EventRequest request);
        Task<PagingEventResponse> GetAllEvent(EventQuery query);
        Task<EventResponse> GetOneEvent(Guid eventId);
        Task UpdateEvent(Guid eventId, UpdateEventRequest request);
        Task<List<EventResponse>> GetEventsByBarId(ObjectQuery query, Guid? barId);
        Task DeleteEvent(Guid eventId);
    }
}
