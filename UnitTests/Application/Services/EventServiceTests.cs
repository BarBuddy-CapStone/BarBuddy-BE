using Application.DTOs.Event;
using Application.DTOs.Events;
using Application.DTOs.Events.EventTime;
using Application.DTOs.Events.EventVoucher;
using Application.Interfaces;
using Application.IService;
using Application.Service;
using AutoMapper;
using Domain.Common;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Application.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFirebase> _firebaseMock;
        private readonly Mock<IEventTimeService> _eventTimeServiceMock;
        private readonly Mock<ILogger<EventService>> _loggerMock;
        private readonly Mock<IEventVoucherService> _eventVoucherServiceMock;
        private readonly Mock<IHttpContextAccessor> _contextAccessorMock;
        private readonly Mock<IAuthentication> _authenticationMock;
        private readonly EventService _eventService;
        public EventServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _firebaseMock = new Mock<IFirebase>();
            _eventTimeServiceMock = new Mock<IEventTimeService>();
            _loggerMock = new Mock<ILogger<EventService>>();
            _eventVoucherServiceMock = new Mock<IEventVoucherService>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _authenticationMock = new Mock<IAuthentication>();
            _eventService = new EventService(
               _mapperMock.Object,
               _unitOfWorkMock.Object,
               _firebaseMock.Object,
               _eventTimeServiceMock.Object,
               _loggerMock.Object,
               _eventVoucherServiceMock.Object,
               _contextAccessorMock.Object,
               _authenticationMock.Object
           );
        }

        [Fact]
        public async Task CreateEvent_WhenBarNotFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var request = new EventRequest
            {
                BarId = barId,
                Images = new List<string> { "base64Image" },
                EventTimeRequest = new List<EventTimeRequest>
                {
                    new EventTimeRequest { DayOfWeek = 1 }
                }    
            };
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
               .Returns(accountId);
            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
               .Returns(new Account { BarId = Guid.NewGuid() });
            // Setup để trả về empty list khi tìm bar
            _unitOfWorkMock.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar>().AsQueryable());
            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                async () => await _eventService.CreateEvent(request)
            );
            Assert.Equal("Không tìm thấy quán Bar", exception.Message);
        }

        [Fact]
        public async Task CreateEvent_WhenUnauthorizedAccess_ThrowsUnAuthorizedException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var request = new EventRequest { BarId = barId };
            var account = new Account { BarId = Guid.NewGuid() }; // Different BarId
            var bar = new Bar { BarId = barId, Status = true };
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
               .Returns(accountId);
            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
               .Returns(account);
            _unitOfWorkMock.Setup(x => x.BarRepository.Get(
                 It.IsAny<Expression<Func<Bar, bool>>>(),
                 It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                 It.IsAny<string>(),
                 It.IsAny<int?>(),
                 It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _eventService.CreateEvent(request)
            );
        }
        [Fact]
        public async Task CreateEvent_WhenNoImages_ThrowsInvalidDataException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var request = new EventRequest
            {
                BarId = barId,
                Images = new List<string>()
            };
            var account = new Account { BarId = barId };
            var bar = new Bar { BarId = barId, Status = true };
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
               .Returns(accountId);
            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
               .Returns(account);
            _unitOfWorkMock.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
               .Returns(new List<Bar> { bar }.AsQueryable());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventService.CreateEvent(request)
            );
        }
        [Fact]
        public async Task CreateEvent_ValidWeeklyEvent_Success()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var validBase64Image = "Z2pwNDV0OTdsNjI1NHVlcXVrN2h1bDBnbG94bG96dzZjcTRxYWVrOTFkMzZnem43ZDY1ZjNmbHRuMzYyN21saWd5NXp0bXFkc24xaTZlc3hmejRra3lqNTk2eWtlYTZ2bDV3ZHhkcDdlazRieHRsazV5dGFtbzJwOTFtZHkwZG0=\r\n";

            var request = new EventRequest
            {
                BarId = barId,
                Images = new List<string> { validBase64Image },
                IsEveryWeek = true,
                EventTimeRequest = new List<EventTimeRequest>
                {
                    new EventTimeRequest { DayOfWeek = 1 }
                }
            };

            var account = new Account { BarId = barId };
            var bar = new Bar { BarId = barId, Status = true };
            var mappedEvent = new Event { EventId = eventId };
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
               .Returns(accountId);
            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
               .Returns(account);
            _unitOfWorkMock.Setup(x => x.BarRepository.Get(
               It.IsAny<Expression<Func<Bar, bool>>>(),
               It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .Returns(new List<Bar> { bar }.AsQueryable());
            _mapperMock.Setup(x => x.Map<Event>(request))
               .Returns(mappedEvent);
            // Mock cho Utils.ConvertBase64ListToFiles
            _unitOfWorkMock.Setup(x => x.BeginTransaction());

            _unitOfWorkMock.Setup(x => x.EventRepository.InsertAsync(It.IsAny<Event>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);
            _firebaseMock.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>()))
               .ReturnsAsync("uploadedImageUrl");
            _eventTimeServiceMock.Setup(x => x.CreateEventTime(It.IsAny<Guid>(), It.IsAny<EventTimeRequest>()))
               .Returns(Task.CompletedTask);
            // Act
            await _eventService.CreateEvent(request);
            // Assert
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.EventRepository.InsertAsync(It.IsAny<Event>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.AtLeastOnce);
            _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
            _eventTimeServiceMock.Verify(x => x.CreateEventTime(It.IsAny<Guid>(), It.IsAny<EventTimeRequest>()), Times.Once);

        }
        private void SetupSuccessfulDependencies(Guid accountId, Account account, Bar bar,
           EventRequest request, Event mappedEvent)
        {
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);
            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
               .Returns(account);
            _unitOfWorkMock.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
               .Returns(new List<Bar> { bar }.AsQueryable());
            _mapperMock.Setup(x => x.Map<Event>(request))
               .Returns(mappedEvent);
            _firebaseMock.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>()))
               .ReturnsAsync("uploadedImageUrl");
        }

        [Fact]
        public async Task CreateEvent_WhenNoEventTimes_ThrowsInvalidDataException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var validBase64Image = "UxdxLxCa2kaTkTCHf4S4uw==";

            var request = new EventRequest
            {
                BarId = barId,
                Images = new List<string> { validBase64Image },
                EventTimeRequest = new List<EventTimeRequest>()
            };

            var account = new Account { BarId = barId };
            var bar = new Bar { BarId = barId, Status = true };
            var mappedEvent = new Event { EventId = eventId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            // Mock mapper
            _mapperMock.Setup(x => x.Map<Event>(request))
                .Returns(mappedEvent);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventService.CreateEvent(request)
            );
        }

        [Fact]
        public async Task CreateEvent_WhenEventTimeHasNeitherDayOfWeekNorDate_ThrowsInvalidDataException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var validBase64Image = "UxdxLxCa2kaTkTCHf4S4uw=="; // Valid base64 string

            var request = new EventRequest
            {
                BarId = barId,
                Images = new List<string> { validBase64Image },
                EventTimeRequest = new List<EventTimeRequest>
                {
                    new EventTimeRequest
                    {
                        DayOfWeek = null,
                        Date = null
                    }
                }
            };
            var mappedEvent = new Event { EventId = Guid.NewGuid(), Images = "" };

            _mapperMock.Setup(x => x.Map<Event>(request))
                .Returns(mappedEvent);

            var account = new Account { BarId = barId };
            var bar = new Bar { BarId = barId, Status = true };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.EventRepository.InsertAsync(It.IsAny<Event>()));

            _unitOfWorkMock.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventService.CreateEvent(request)
            );
            Assert.Equal("Ngày trong tuần và ngày diễn ra sự kiện, cả 2 không thể không có giá trị cùng một lúc !", exception.Message);
        }

        [Fact]
        public async Task CreateEvent_WhenEventTimeHasBothDayOfWeekAndDate_ThrowsInvalidDataException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var validBase64Image = "UxdxLxCa2kaTkTCHf4S4uw==";

            var request = new EventRequest
            {
                BarId = barId,
                Images = new List<string> { validBase64Image },
                EventTimeRequest = new List<EventTimeRequest>
                {
                    new EventTimeRequest 
                    { 
                        DayOfWeek = 1,
                        Date = DateTime.Now
                    }
                },
                IsEveryWeek = true
            };

            var account = new Account { BarId = barId };
            var bar = new Bar { BarId = barId, Status = true };
            var mappedEvent = new Event { EventId = eventId, Images = "" };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            _mapperMock.Setup(x => x.Map<Event>(request))
                .Returns(mappedEvent);

            _unitOfWorkMock.Setup(x => x.EventRepository.InsertAsync(It.IsAny<Event>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _firebaseMock.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("uploaded_image_url");

            _unitOfWorkMock.Setup(x => x.BeginTransaction());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventService.CreateEvent(request)
            );
            Assert.Equal("Ngày trong tuần và ngày diễn ra sự kiện, cả 2 không thể có giá trị cùng một lúc !", exception.Message);
        }

        [Fact]
        public async Task DeleteEvent_WhenEventNotFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            // Thêm mock cho AccountRepository.GetByID
            var account = new Account { BarId = barId };
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.EventRepository.Get(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Event>().AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _eventService.DeleteEvent(eventId)
            );
            Assert.Equal("Không tìm thấy event !", exception.Message);
        }

        [Fact]
        public async Task DeleteEvent_WhenUnauthorizedAccess_ThrowsUnAuthorizedException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();

            var existingEvent = new Event 
            { 
                EventId = eventId,
                BarId = barId,
                IsDeleted = false
            };

            var account = new Account { BarId = differentBarId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.EventRepository.Get(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Event> { existingEvent }.AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _eventService.DeleteEvent(eventId)
            );
            Assert.Equal("Bạn không có quyền truy cập vào quán bar này !", exception.Message);
        }

        [Fact]
        public async Task DeleteEvent_WhenValidRequest_Success()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            var existingEvent = new Event 
            { 
                EventId = eventId,
                BarId = barId,
                IsDeleted = false
            };

            var account = new Account { BarId = barId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.EventRepository.Get(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Event> { existingEvent }.AsQueryable());

            _unitOfWorkMock.Setup(x => x.EventRepository.UpdateRangeAsync(It.IsAny<Event>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _eventService.DeleteEvent(eventId);

            // Assert
            _unitOfWorkMock.Verify(x => x.EventRepository.UpdateRangeAsync(
                It.Is<Event>(e => e.IsDeleted == true)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteEvent_WhenSystemError_ThrowsInternalServerErrorException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            var existingEvent = new Event 
            { 
                EventId = eventId,
                BarId = barId,
                IsDeleted = false
            };

            var account = new Account { BarId = barId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.EventRepository.Get(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Event> { existingEvent }.AsQueryable());

            _unitOfWorkMock.Setup(x => x.EventRepository.UpdateRangeAsync(It.IsAny<Event>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventService.DeleteEvent(eventId)
            );
            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }

        [Fact]
        public async Task GetAllEvent_WhenNoEventsFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var query = new EventQuery();
            _unitOfWorkMock.Setup(x => x.EventRepository.GetAsync(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Event>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _eventService.GetAllEvent(query)
            );
        }

        [Fact]
        public async Task GetAllEvent_WithSearchParameter_ReturnsFilteredEvents()
        {
            // Arrange
            var query = new EventQuery { Search = "Test Event" };
            var events = new List<Event>
            {
                new Event 
                { 
                    EventId = Guid.NewGuid(), 
                    EventName = "Test Event 1", 
                    IsDeleted = false, 
                    IsHide = false,
                    TimeEvent = new List<TimeEvent> 
                    { 
                        new TimeEvent 
                        { 
                            Date = DateTime.Now.AddDays(1),
                            StartTime = new TimeSpan(10, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0)
                        } 
                    }
                },
                new Event 
                { 
                    EventId = Guid.NewGuid(), 
                    EventName = "Other Event", 
                    IsDeleted = false, 
                    IsHide = false,
                    TimeEvent = new List<TimeEvent> 
                    { 
                        new TimeEvent 
                        { 
                            Date = DateTime.Now.AddDays(2),
                            StartTime = new TimeSpan(10, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0)
                        } 
                    }
                }
            };

            _unitOfWorkMock.Setup(x => x.EventRepository.GetAsync(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(events.AsQueryable());

            // Mock cho Map<List<EventResponse>>
            _mapperMock.Setup(x => x.Map<List<EventResponse>>(It.IsAny<List<Event>>()))
                .Returns((List<Event> src) => src.Select(e => new EventResponse 
                { 
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventTimeResponses = new List<EventTimeResponse>() // Để trống, sẽ được map ở dưới
                }).ToList());

            // Mock cho Map<List<EventTimeResponse>>
            _mapperMock.Setup(x => x.Map<List<EventTimeResponse>>(It.IsAny<ICollection<TimeEvent>>()))
                .Returns((ICollection<TimeEvent> timeEvents) => timeEvents.Select(t => new EventTimeResponse
                {
                    Date = t.Date,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime
                }).ToList());

            // Act
            var result = await _eventService.GetAllEvent(query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.EventResponses.Where(e => e.EventName.Contains("Test Event")));
            Assert.All(result.EventResponses, e => Assert.NotEmpty(e.EventTimeResponses));
        }

        [Fact]
        public async Task GetAllEvent_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new EventQuery { PageIndex = 2, PageSize = 2 };
            var events = new List<Event>();
            for (int i = 1; i <= 5; i++)
            {
                events.Add(new Event 
                { 
                    EventId = Guid.NewGuid(),
                    EventName = $"Event {i}",
                    IsDeleted = false,
                    IsHide = false,
                    TimeEvent = new List<TimeEvent> 
                    { 
                        new TimeEvent 
                        { 
                            Date = DateTime.Now.AddDays(i),
                            StartTime = new TimeSpan(10, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0)
                        } 
                    }
                });
            }

            _unitOfWorkMock.Setup(x => x.EventRepository.GetAsync(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(events.AsQueryable());

            // Mock cho Map<List<EventResponse>>
            _mapperMock.Setup(x => x.Map<List<EventResponse>>(It.IsAny<List<Event>>()))
                .Returns((List<Event> src) => src.Select(e => new EventResponse 
                { 
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventTimeResponses = new List<EventTimeResponse>() // Để trống, sẽ được map ở dưới
                }).ToList());

            // Mock cho Map<List<EventTimeResponse>>
            _mapperMock.Setup(x => x.Map<List<EventTimeResponse>>(It.IsAny<ICollection<TimeEvent>>()))
                .Returns((ICollection<TimeEvent> timeEvents) => timeEvents.Select(t => new EventTimeResponse
                {
                    Date = t.Date,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime
                }).ToList());

            // Act
            var result = await _eventService.GetAllEvent(query);

            // Assert
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(5, result.TotalItems);
            
            // Verify r���ng mỗi event có EventTimeResponses
            Assert.All(result.EventResponses, e => Assert.NotEmpty(e.EventTimeResponses));
        }

        [Fact]
        public async Task GetAllEvent_WithWeeklyAndDateEvents_ReturnsSortedEvents()
        {
            // Arrange
            var query = new EventQuery();
            var events = new List<Event>
            {
                new Event 
                { 
                    EventId = Guid.NewGuid(),
                    EventName = "Date Event",
                    IsDeleted = false,
                    IsHide = false,
                    TimeEvent = new List<TimeEvent> 
                    { 
                        new TimeEvent 
                        { 
                            Date = DateTime.Now.AddDays(1),
                            StartTime = new TimeSpan(10, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0)
                        } 
                    }
                },
                new Event 
                { 
                    EventId = Guid.NewGuid(),
                    EventName = "Weekly Event",
                    IsDeleted = false,
                    IsHide = false,
                    TimeEvent = new List<TimeEvent> 
                    { 
                        new TimeEvent 
                        { 
                            DayOfWeek = 1,
                            StartTime = new TimeSpan(10, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0)
                        } 
                    }
                }
            };

            _unitOfWorkMock.Setup(x => x.EventRepository.GetAsync(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(events.AsQueryable());

            // Mock cho Map<List<EventResponse>>
            _mapperMock.Setup(x => x.Map<List<EventResponse>>(It.IsAny<List<Event>>()))
                .Returns((List<Event> src) => src.Select(e => new EventResponse 
                { 
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventTimeResponses = new List<EventTimeResponse>() // Để trống, sẽ được map ở dưới
                }).ToList());

            // Mock cho Map<List<EventTimeResponse>>
            _mapperMock.Setup(x => x.Map<List<EventTimeResponse>>(It.IsAny<ICollection<TimeEvent>>()))
                .Returns((ICollection<TimeEvent> timeEvents) => timeEvents.Select(t => new EventTimeResponse
                {
                    DayOfWeek = t.DayOfWeek,
                    Date = t.Date,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime
                }).ToList());

            // Act
            var result = await _eventService.GetAllEvent(query);

            // Assert
            Assert.Equal("Date Event", result.EventResponses.First().EventName);
            Assert.All(result.EventResponses, e => Assert.NotEmpty(e.EventTimeResponses));
        }

        [Fact]
        public async Task GetEventsByBarId_WhenBarIdIsNull_ThrowsInvalidDataException()
        {
            // Arrange
            var query = new ObjectQuery();
            Guid? barId = null;

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventService.GetEventsByBarId(query, barId)
            );
        }

        [Fact]
        public async Task GetEventsByBarId_WhenNoEventsFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var query = new ObjectQuery();
            var barId = Guid.NewGuid();

            _unitOfWorkMock.Setup(x => x.EventRepository.GetAsync(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Event>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _eventService.GetEventsByBarId(query, barId)
            );
        }

        [Fact]
        public async Task GetEventsByBarId_WhenEventsExist_ReturnsEventResponses()
        {
            // Arrange
            var query = new ObjectQuery { PageIndex = 1, PageSize = 2 };
            var barId = Guid.NewGuid();
            var events = new List<Event>
            {
                new Event 
                { 
                    EventId = Guid.NewGuid(),
                    BarId = barId,
                    EventName = "Event 1",
                    IsDeleted = false,
                    IsHide = false,
                    TimeEvent = new List<TimeEvent> 
                    { 
                        new TimeEvent 
                        { 
                            Date = DateTime.Now.AddDays(1),
                            StartTime = new TimeSpan(10, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0)
                        } 
                    },
                    EventVoucher = new EventVoucher
                    {
                        EventVoucherId = Guid.NewGuid(),
                        EventVoucherName = "Voucher 1"
                    }
                },
                new Event 
                { 
                    EventId = Guid.NewGuid(),
                    BarId = barId,
                    EventName = "Event 2",
                    IsDeleted = false,
                    IsHide = false,
                    TimeEvent = new List<TimeEvent> 
                    { 
                        new TimeEvent 
                        { 
                            DayOfWeek = 1,
                            StartTime = new TimeSpan(10, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0)
                        } 
                    },
                    EventVoucher = new EventVoucher
                    {
                        EventVoucherId = Guid.NewGuid(),
                        EventVoucherName = "Voucher 2"
                    }
                }
            };

            _unitOfWorkMock.Setup(x => x.EventRepository.GetAsync(
                It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(events.AsQueryable());

            // Mock cho Map<List<EventResponse>>
            _mapperMock.Setup(x => x.Map<List<EventResponse>>(It.Is<IEnumerable<Event>>(e => e.SequenceEqual(events))))
                .Returns(events.Select(e => new EventResponse 
                { 
                    BarId = barId,
                    BarName = "Bar Test1",
                    Description = "Description",
                    EventId = e.EventId,
                    EventName = e.EventName,
                    EventTimeResponses = new List<EventTimeResponse>(),
                    EventVoucherResponse = null
                }).ToList());

            // Mock cho Map<List<EventTimeResponse>>
            _mapperMock.Setup(x => x.Map<List<EventTimeResponse>>(It.IsAny<ICollection<TimeEvent>>()))
                .Returns((ICollection<TimeEvent> timeEvents) => timeEvents.Select(t => new EventTimeResponse
                {
                    DayOfWeek = t.DayOfWeek,
                    Date = t.Date,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime
                }).ToList());

            // Mock cho Map<EventVoucherResponse>
            _mapperMock.Setup(x => x.Map<EventVoucherResponse>(It.IsAny<EventVoucher>()))
                .Returns((EventVoucher voucher) => new EventVoucherResponse
                {
                    EventVoucherId = voucher.EventVoucherId,
                    EventVoucherName = voucher.EventVoucherName
                });

            // Act
            var result = await _eventService.GetEventsByBarId(query, barId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, e => 
            {
                Assert.NotEmpty(e.EventTimeResponses);
                Assert.NotNull(e.EventVoucherResponse);
            });
            Assert.Equal("Event 1", result[0].EventName);
            Assert.Equal("Event 2", result[1].EventName);

            // Verify mapper được gọi đúng số lần
            _mapperMock.Verify(x => x.Map<List<EventTimeResponse>>(It.IsAny<ICollection<TimeEvent>>()), Times.Exactly(2));
            _mapperMock.Verify(x => x.Map<EventVoucherResponse>(It.IsAny<EventVoucher>()), Times.Exactly(2));
        }
    }
}
