using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Events.EventTime;
using Application.IService;
using Application.Service;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Moq;
using Xunit;

namespace UnitTests.Application.Services
{
    public class EventTimeServiceTest
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IEventVoucherService> _eventVoucherServiceMock;
        private readonly EventTimeService _eventTimeService;

        public EventTimeServiceTest()
        {
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _eventVoucherServiceMock = new Mock<IEventVoucherService>();
            _eventTimeService = new EventTimeService(_mapperMock.Object, _unitOfWorkMock.Object, _eventVoucherServiceMock.Object);
        }

        [Fact]
        public async Task CreateEventTime_Success()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new EventTimeRequest
            {
                // Thêm các property cần thiết
            };
            var timeEvent = new TimeEvent();

            _mapperMock.Setup(x => x.Map<TimeEvent>(request))
                      .Returns(timeEvent);

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.InsertAsync(It.IsAny<TimeEvent>()))
                          .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                          .Returns(Task.CompletedTask);

            // Act
            await _eventTimeService.CreateEventTime(eventId, request);

            // Assert
            _mapperMock.Verify(x => x.Map<TimeEvent>(request), Times.Once);
            _unitOfWorkMock.Verify(x => x.TimeEventRepository.InsertAsync(It.Is<TimeEvent>(te => te.EventId == eventId)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateEventTime_ThrowsException_WhenInternalErrorOccurs()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new EventTimeRequest();
            var timeEvent = new TimeEvent();

            _mapperMock.Setup(x => x.Map<TimeEvent>(request))
                      .Returns(timeEvent);

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.InsertAsync(It.IsAny<TimeEvent>()))
                          .ThrowsAsync(new CustomException.InternalServerErrorException("Test error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventTimeService.CreateEventTime(eventId, request)
            );
        }

        [Fact]
        public async Task CreateEventTime_SetsCorrectEventId()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new EventTimeRequest();
            var timeEvent = new TimeEvent();
            TimeEvent capturedTimeEvent = null;

            _mapperMock.Setup(x => x.Map<TimeEvent>(request))
                      .Returns(timeEvent);

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.InsertAsync(It.IsAny<TimeEvent>()))
                          .Callback<TimeEvent>(te => capturedTimeEvent = te)
                          .Returns(Task.CompletedTask);

            // Act
            await _eventTimeService.CreateEventTime(eventId, request);

            // Assert
            Assert.Equal(eventId, capturedTimeEvent.EventId);
        }

        [Fact]
        public async Task UpdateEventTime_ValidatesWeeklyEventCorrectly()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new List<UpdateEventTimeRequest>
            {
                new UpdateEventTimeRequest
                {
                    DayOfWeek = 1, // Monday
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(12)
                }
            };

            // Setup mock cho BarTime với đầy đủ relationship
            var barTimes = new List<BarTime> 
            {
                new BarTime
                {
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(23),
                    Bar = new Bar 
                    { 
                        Event = new List<Event> 
                        { 
                            new Event { EventId = eventId, IsDeleted = false } 
                        }
                    }
                }
            }.AsQueryable();

            // Setup mock cho TimeEventRepository
            var timeEvents = new List<TimeEvent>().AsQueryable();
            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(timeEvents);

            // Setup mock cho BarTimeRepository
            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            // Setup các mock khác cần thiết
            _mapperMock.Setup(x => x.Map<EventTimeRequest>(It.IsAny<UpdateEventTimeRequest>()))
                .Returns(new EventTimeRequest());

            _mapperMock.Setup(x => x.Map(It.IsAny<UpdateEventTimeRequest>(), It.IsAny<TimeEvent>()))
                .Returns((UpdateEventTimeRequest req, TimeEvent ev) => ev);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventTimeService.UpdateEventTime(eventId, false, request)
            );
            
            Assert.Equal("Data không hợp lệ khi sự kiện không diễn ra hàng tuần mà có ngày diễn ra !", exception.Message);
        }

        [Fact]
        public async Task UpdateEventTime_ValidatesNonWeeklyEventCorrectly()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new List<UpdateEventTimeRequest>
            {
                new UpdateEventTimeRequest
                {
                    Date = DateTime.Now.Date,
                    DayOfWeek = null,
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(12)
                }
            };

            // Setup mock cho BarTime với đầy đủ relationship
            var barTimes = new List<BarTime> 
            {
                new BarTime
                {
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(23),
                    Bar = new Bar 
                    { 
                        Event = new List<Event> 
                        { 
                            new Event { EventId = eventId, IsDeleted = false } 
                        }
                    }
                }
            }.AsQueryable();

            // Setup mock cho TimeEventRepository
            var timeEvents = new List<TimeEvent>().AsQueryable();
            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(timeEvents);

            // Setup mock cho BarTimeRepository
            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            // Setup mock cho mapper
            var mappedEventTimeRequest = new EventTimeRequest
            {
                Date = DateTime.Now.Date,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12)
            };

            _mapperMock.Setup(x => x.Map<EventTimeRequest>(It.IsAny<UpdateEventTimeRequest>()))
                .Returns(mappedEventTimeRequest);

            var mappedTimeEvent = new TimeEvent
            {
                EventId = eventId,
                Date = DateTime.Now.Date,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12)
            };

            _mapperMock.Setup(x => x.Map<TimeEvent>(It.IsAny<EventTimeRequest>()))
                .Returns(mappedTimeEvent);

            _mapperMock.Setup(x => x.Map(It.IsAny<UpdateEventTimeRequest>(), It.IsAny<TimeEvent>()))
                .Returns((UpdateEventTimeRequest req, TimeEvent ev) => mappedTimeEvent);

            // Setup mock cho SaveAsync
            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Setup mock cho TimeEventRepository.InsertAsync
            _unitOfWorkMock.Setup(x => x.TimeEventRepository.InsertAsync(It.IsAny<TimeEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            await _eventTimeService.UpdateEventTime(eventId, false, request);

            // Assert
            _unitOfWorkMock.Verify(x => x.TimeEventRepository.InsertAsync(It.IsAny<TimeEvent>()), Times.Once());
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task UpdateEventTime_HandlesDeletionCorrectly()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var existingTimeEventId = Guid.NewGuid();
            var request = new List<UpdateEventTimeRequest>();

            var existingTimeEvents = new List<TimeEvent>
            {
                new TimeEvent { TimeEventId = existingTimeEventId, EventId = eventId }
            };

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(existingTimeEvents.AsQueryable());

            SetupCommonMocks();

            // Act
            await _eventTimeService.UpdateEventTime(eventId, false, request);

            // Assert
            _unitOfWorkMock.Verify(x => x.TimeEventRepository.DeleteAsync(existingTimeEventId), Times.Once());
        }

        [Fact]
        public async Task UpdateEventTime_HandlesUpdateCorrectly()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var timeEventId = Guid.NewGuid();
            var request = new List<UpdateEventTimeRequest>
            {
                new UpdateEventTimeRequest
                {
                    TimeEventId = timeEventId,
                    Date = DateTime.Now.Date,
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(12)
                }
            };

            var existingTimeEvent = new TimeEvent 
            { 
                TimeEventId = timeEventId, 
                EventId = eventId 
            };

            var barTimes = new List<BarTime> 
            {
                new BarTime
                {
                    StartTime = TimeSpan.FromHours(9), // Mở cửa sớm hơn event
                    EndTime = TimeSpan.FromHours(23), // Đóng cửa muộn hơn event
                    Bar = new Bar 
                    { 
                        Event = new List<Event> 
                        { 
                            new Event { EventId = eventId } 
                        }
                    }
                }
            }.AsQueryable();

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.GetByID(timeEventId))
                           .Returns(existingTimeEvent);

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                           .Returns(new List<TimeEvent> { existingTimeEvent }.AsQueryable());

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                           .Returns(barTimes);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                           .Returns(Task.CompletedTask);

            _mapperMock.Setup(x => x.Map<EventTimeRequest>(It.IsAny<UpdateEventTimeRequest>()))
                       .Returns(new EventTimeRequest());

            _mapperMock.Setup(x => x.Map(It.IsAny<UpdateEventTimeRequest>(), It.IsAny<TimeEvent>()))
                       .Returns((UpdateEventTimeRequest req, TimeEvent ev) => ev);

            // Act
            await _eventTimeService.UpdateEventTime(eventId, false, request);

            // Assert
            _unitOfWorkMock.Verify(x => x.TimeEventRepository.UpdateRangeAsync(It.IsAny<TimeEvent>()), Times.Once());
        }

        [Fact]
        public async Task UpdateEventTime_ThrowsException_WhenEventTimeOutsideBarOpenHours()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new List<UpdateEventTimeRequest>
            {
                new UpdateEventTimeRequest
                {
                    Date = DateTime.Now.Date,
                    StartTime = TimeSpan.FromHours(8), // Sớm hơn giờ mở cửa
                    EndTime = TimeSpan.FromHours(10)
                }
            };

            var barTimes = new List<BarTime> 
            {
                new BarTime
                {
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(23),
                    Bar = new Bar 
                    { 
                        Event = new List<Event> 
                        { 
                            new Event { EventId = eventId, IsDeleted = false } 
                        }
                    }
                }
            }.AsQueryable();

            // Setup mock cho TimeEventRepository
            var timeEvents = new List<TimeEvent>().AsQueryable();
            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(timeEvents);

            // Setup mock cho BarTimeRepository
            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            // Setup các mock khác cần thiết
            _mapperMock.Setup(x => x.Map<EventTimeRequest>(It.IsAny<UpdateEventTimeRequest>()))
                .Returns(new EventTimeRequest());

            _mapperMock.Setup(x => x.Map(It.IsAny<UpdateEventTimeRequest>(), It.IsAny<TimeEvent>()))
                .Returns((UpdateEventTimeRequest req, TimeEvent ev) => ev);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventTimeService.UpdateEventTime(eventId, false, request)
            );
            
            Assert.Equal("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!", exception.Message);
        }

        private void SetupCommonMocks()
        {
            var barTimes = new List<BarTime>().AsQueryable();

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                           .Returns(Task.CompletedTask);

            _mapperMock.Setup(x => x.Map<EventTimeRequest>(It.IsAny<UpdateEventTimeRequest>()))
                       .Returns(new EventTimeRequest());

            _mapperMock.Setup(x => x.Map(It.IsAny<UpdateEventTimeRequest>(), It.IsAny<TimeEvent>()))
                       .Returns((UpdateEventTimeRequest req, TimeEvent ev) => ev);
        }

        [Fact]
        public async Task UpdateEventTime_ValidatesBothDateAndDayOfWeek()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new List<UpdateEventTimeRequest>
            {
                new UpdateEventTimeRequest
                {
                    Date = DateTime.Now,
                    DayOfWeek = 1,
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(12)
                }
            };

            // Setup mock cho BarTime với đầy đủ relationship
            var barTimes = new List<BarTime> 
            {
                new BarTime
                {
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(23),
                    Bar = new Bar 
                    { 
                        Event = new List<Event> 
                        { 
                            new Event { EventId = eventId, IsDeleted = false } 
                        }
                    }
                }
            }.AsQueryable();

            // Setup mock cho TimeEventRepository
            var timeEvents = new List<TimeEvent>().AsQueryable();
            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(timeEvents);

            // Setup mock cho BarTimeRepository
            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            // Setup các mock khác cần thiết
            _mapperMock.Setup(x => x.Map<EventTimeRequest>(It.IsAny<UpdateEventTimeRequest>()))
                .Returns(new EventTimeRequest());

            _mapperMock.Setup(x => x.Map(It.IsAny<UpdateEventTimeRequest>(), It.IsAny<TimeEvent>()))
                .Returns((UpdateEventTimeRequest req, TimeEvent ev) => ev);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventTimeService.UpdateEventTime(eventId, true, request)
            );
            
            Assert.Equal("Ngày trong tuần và ngày diễn ra sự kiện, cả 2 không thể có giá trị cùng một lúc !", exception.Message);
        }

        [Fact]
        public async Task UpdateEventTime_ValidatesDateAndDayOfWeekCombination()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new List<UpdateEventTimeRequest>
            {
                new UpdateEventTimeRequest
                {
                    Date = null, // Bỏ Date
                    DayOfWeek = null, // Bỏ DayOfWeek
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(12)
                }
            };

            // Setup tương tự như trên...
            SetupMocksForValidation(eventId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventTimeService.UpdateEventTime(eventId, false, request)
            );
            
            Assert.Equal("Ngày trong tuần và ngày diễn ra sự kiện, cả 2 không thể không có giá trị cùng một lúc !", exception.Message);
        }

        private void SetupMocksForValidation(Guid eventId)
        {
            var barTimes = new List<BarTime> 
            {
                new BarTime
                {
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(23),
                    Bar = new Bar 
                    { 
                        Event = new List<Event> 
                        { 
                            new Event { EventId = eventId, IsDeleted = false } 
                        }
                    }
                }
            }.AsQueryable();

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<TimeEvent>().AsQueryable());

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            _mapperMock.Setup(x => x.Map<EventTimeRequest>(It.IsAny<UpdateEventTimeRequest>()))
                .Returns(new EventTimeRequest());

            _mapperMock.Setup(x => x.Map(It.IsAny<UpdateEventTimeRequest>(), It.IsAny<TimeEvent>()))
                .Returns((UpdateEventTimeRequest req, TimeEvent ev) => ev);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task DeleteEventTime_DeletesExistingEventTimes()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var timeEventId1 = Guid.NewGuid();
            var timeEventId2 = Guid.NewGuid();
            var eventTimeIds = new List<Guid> { timeEventId1, timeEventId2 };

            var existingTimeEvents = new List<TimeEvent>
            {
                new TimeEvent 
                { 
                    TimeEventId = timeEventId1, 
                    EventId = eventId,
                    Event = new Event { IsDeleted = false }
                },
                new TimeEvent 
                { 
                    TimeEventId = timeEventId2, 
                    EventId = eventId,
                    Event = new Event { IsDeleted = false }
                }
            }.AsQueryable();

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(existingTimeEvents);

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.DeleteAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _eventTimeService.DeleteEventTime(eventId, eventTimeIds);

            // Assert
            _unitOfWorkMock.Verify(x => x.TimeEventRepository.DeleteAsync(timeEventId1), Times.Once);
            _unitOfWorkMock.Verify(x => x.TimeEventRepository.DeleteAsync(timeEventId2), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteEventTime_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var timeEventId = Guid.NewGuid();
            var eventTimeIds = new List<Guid> { timeEventId };

            var existingTimeEvents = new List<TimeEvent>
            {
                new TimeEvent 
                { 
                    TimeEventId = timeEventId, 
                    EventId = eventId,
                    Event = new Event { IsDeleted = false }
                }
            }.AsQueryable();

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.IsAny<Expression<Func<TimeEvent, bool>>>(),
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(existingTimeEvents);

            _unitOfWorkMock.Setup(x => x.TimeEventRepository.DeleteAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Test error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventTimeService.DeleteEventTime(eventId, eventTimeIds)
            );

            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }

        [Fact]
        public async Task DeleteEventTime_SkipsDeletedEvents()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var timeEventId = Guid.NewGuid();
            var eventTimeIds = new List<Guid> { timeEventId };

            // Setup mock cho TimeEventRepository.Get với điều kiện filter chính xác
            _unitOfWorkMock.Setup(x => x.TimeEventRepository.Get(
                It.Is<Expression<Func<TimeEvent, bool>>>(expr => true), // Chấp nhận mọi filter expression
                It.IsAny<Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns((Expression<Func<TimeEvent, bool>> filter,
                        Func<IQueryable<TimeEvent>, IOrderedQueryable<TimeEvent>> orderBy,
                        string includeProperties,
                        int? pageIndex,
                        int? pageSize) =>
                {
                    // Trả về empty collection để giả lập việc không tìm thấy TimeEvent hợp lệ
                    return new List<TimeEvent>().AsQueryable();
                });

            // Setup mock cho DeleteAsync để verify không được gọi
            _unitOfWorkMock.Setup(x => x.TimeEventRepository.DeleteAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Setup mock cho SaveAsync để verify không được gọi
            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _eventTimeService.DeleteEventTime(eventId, eventTimeIds);

            // Assert
            _unitOfWorkMock.Verify(x => x.TimeEventRepository.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
        }
    }
}
