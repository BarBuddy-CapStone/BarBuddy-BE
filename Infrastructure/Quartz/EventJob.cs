using Application.IService;
using Domain.Constants;
using Domain.CustomException;
using Domain.IRepository;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Quartz
{
    public class EventJob : IJob
    {
        private readonly IEventService _eventService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EventJob> _logger;
        public EventJob(IEventService eventService, IUnitOfWork unitOfWork, ILogger<EventJob> logger)
        {
            _eventService = eventService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {

                DateTimeOffset dateTime = DateTimeOffset.Now;
                TimeSpan getTime = TimeSpan.FromHours(dateTime.TimeOfDay.Hours)
                                            .Add(TimeSpan.FromMinutes(dateTime.TimeOfDay.Minutes))
                                            .Add(TimeSpan.FromSeconds(dateTime.TimeOfDay.Seconds));

                var getEventFuture = await _unitOfWork.EventRepository
                                                .GetAsync(filter: x => x.TimeEvent.Any(x => x.Date != null &&
                                                                    x.Date.Value.Date == dateTime.Date &&
                                                                    x.EndTime <= getTime) &&
                                                                  x.IsDeleted == PrefixKeyConstant.FALSE,
                                                     includeProperties: "TimeEvent");

                foreach (var events in getEventFuture)
                {
                    var isEndEvent = events.TimeEvent.Where(x => x.Date.Value.Date == dateTime.Date && x.EndTime <= getTime).ToList();
                    var isGoingOnEvent = events.TimeEvent.Where(x => x.Date.Value.Date == dateTime.Date && x.EndTime > getTime).ToList();
                    var futureEvent = events.TimeEvent.Where(x => x.Date.Value.Date > dateTime.Date).ToList();

                    if (!isEndEvent.IsNullOrEmpty() && isGoingOnEvent.Count == 0 && futureEvent.Count == 0)
                    {
                        events.IsDeleted = PrefixKeyConstant.TRUE;
                        await _unitOfWork.EventRepository.UpdateAsync(events);
                        await Task.Delay(10);
                        await _unitOfWork.SaveAsync();
                        _logger.LogInformation($"{events.EventName} da update sts");
                    }
                };
            }
            catch
            {
                throw new CustomException.InternalServerErrorException("Lỗi hệ thống !");
            }
        }
    }
}
