using Application.DTOs.BookingTable;
using Application.DTOs.TableType;
using Application.Interfaces;
using Application.Service;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.ML.Transforms;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Application.Services
{
    public class BookingTableServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IBookingHubService> _bookingHubMock;
        private readonly Mock<ILogger<BookingTableService>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuthentication> _authenticationMock;
        private readonly BookingTableService _bookingTableService;

        public BookingTableServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _bookingHubMock = new Mock<IBookingHubService>();
            _loggerMock = new Mock<ILogger<BookingTableService>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _authenticationMock = new Mock<IAuthentication>();

            _bookingTableService = new BookingTableService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _memoryCacheMock.Object,
                _bookingHubMock.Object,
                _loggerMock.Object,
                _httpContextAccessorMock.Object,
                _authenticationMock.Object
            );
        }

        [Fact]
        public async Task FilterTableTypeReponse_WhenValidRequest_ShouldReturnFilterTableTypeReponse()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();
            var requestDate = DateTime.Now.Date.AddDays(1);
            var requestTime = new TimeSpan(14, 0, 0);

            var request = new FilterTableDateTimeRequest
            {
                BarId = barId,
                TableTypeId = tableTypeId,
                Date = requestDate,
                Time = requestTime
            };

            var bar = new Bar { BarId = barId };
            var barTimes = new List<BarTime>
        {
            new BarTime
            {
                BarId = barId,
                DayOfWeek = (int)requestDate.DayOfWeek,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(22, 0, 0)
            }
        };

            var tables = new List<Table>
        {
            new Table
            {
                TableId = Guid.NewGuid(),
                TableName = "Table 1",
                TableTypeId = tableTypeId,
                TableType = new TableType { BarId = barId, Bar = bar },
                BookingTables = new List<BookingTable>()
            }
        };

            var expectedResponse = new FilterTableTypeReponse
            {
                TableTypeId = tableTypeId,
                BookingTables = new List<FilterBkTableResponse>()
            };

            _unitOfWorkMock.Setup(x => x.BarRepository.GetByID(barId))
                .Returns(bar);

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            _unitOfWorkMock.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tables);

            _mapperMock.Setup(x => x.Map<FilterTableTypeReponse>(It.IsAny<TableType>()))
                .Returns(expectedResponse);

            // Act
            var result = await _bookingTableService.FilterTableTypeReponse(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.BookingTables);
            Assert.Single(result.BookingTables);
            Assert.Equal(tableTypeId, result.TableTypeId);
        }

        [Fact]
        public async Task FilterTableTypeReponse_WhenBarTimeNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var request = new FilterTableDateTimeRequest
            {
                BarId = Guid.NewGuid(),
                Date = DateTime.Now.Date,
                Time = new TimeSpan(14, 0, 0)
            };

            _unitOfWorkMock.Setup(x => x.BarRepository.GetByID(request.BarId))
                .Returns(new Bar());

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<BarTime>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() =>
                _bookingTableService.FilterTableTypeReponse(request));
            Assert.Equal("Không tìm thấy khung giờ của quán Bar !", exception.Message);
        }

        [Fact]
        public async Task FilterTableTypeReponse_WhenTableNotFound_ShouldThrowInvalidDataException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var request = new FilterTableDateTimeRequest
            {
                BarId = barId,
                Date = DateTime.Now.Date,
                Time = new TimeSpan(14, 0, 0)
            };

            var barTimes = new List<BarTime>
        {
            new BarTime
            {
                BarId = barId,
                DayOfWeek = (int)request.Date.DayOfWeek,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(22, 0, 0)
            }
        };

            _unitOfWorkMock.Setup(x => x.BarRepository.GetByID(barId))
                .Returns(new Bar());

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            _unitOfWorkMock.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(() =>
                _bookingTableService.FilterTableTypeReponse(request));
        }

        [Fact]
        public async Task FilterTableTypeReponse_WhenOutsideOperatingHours_ShouldThrowInvalidDataException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var request = new FilterTableDateTimeRequest
            {
                BarId = barId,
                Date = DateTime.Now.Date,
                Time = new TimeSpan(23, 0, 0)
            };

            var barTimes = new List<BarTime>
        {
            new BarTime
            {
                BarId = barId,
                DayOfWeek = (int)request.Date.DayOfWeek,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(22, 0, 0)
            }
        };

            _unitOfWorkMock.Setup(x => x.BarRepository.GetByID(barId))
                .Returns(new Bar());

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(() =>
                _bookingTableService.FilterTableTypeReponse(request));
        }

        [Fact]
        public async Task HoldTable_WhenValidRequest_ShouldReturnTableHoldInfo()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new TablesRequest
            {
                BarId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                Date = DateTime.Now.Date.AddDays(1),
                Time = new TimeSpan(14, 0, 0)
            };

            var table = new Table
            {
                TableId = request.TableId,
                TableName = "Test Table",
                TableType = new TableType
                {
                    BarId = request.BarId,
                    Bar = new Bar { BarId = request.BarId }
                }
            };

            var barTimes = new List<BarTime>
    {
        new BarTime
        {
            BarId = request.BarId,
            DayOfWeek = (int)request.Date.DayOfWeek,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(22, 0, 0)
        }
    };

            // Setup Authentication
            _authenticationMock
                .Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            // Setup Repository
            _unitOfWorkMock
                .Setup(x => x.TableRepository.Get(
                    It.IsAny<Expression<Func<Table, bool>>>(),
                    It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .Returns(new List<Table> { table });

            _unitOfWorkMock
                .Setup(x => x.BarTimeRepository.Get(
                    It.IsAny<Expression<Func<BarTime, bool>>>(),
                    It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .Returns(barTimes);

            // Setup Memory Cache
            var memoryCacheEntryMock = new Mock<ICacheEntry>();
            var cacheDict = new Dictionary<Guid, TableHoldInfo>();

            // Setup Value và các property cho ICacheEntry
            memoryCacheEntryMock.SetupProperty(e => e.Value, cacheDict);
            memoryCacheEntryMock.SetupProperty(e => e.AbsoluteExpiration);
            memoryCacheEntryMock.SetupProperty(e => e.AbsoluteExpirationRelativeToNow);
            memoryCacheEntryMock.SetupProperty(e => e.Priority);
            memoryCacheEntryMock.SetupProperty(e => e.Size);
            memoryCacheEntryMock.SetupProperty(e => e.SlidingExpiration);

            memoryCacheEntryMock
                .SetupGet(x => x.ExpirationTokens)
                .Returns(new List<IChangeToken>());

            memoryCacheEntryMock
                .SetupGet(x => x.PostEvictionCallbacks)
                .Returns(new List<PostEvictionCallbackRegistration>());

            // Setup IMemoryCache
            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(memoryCacheEntryMock.Object);

            object expectedValue = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out expectedValue))
                .Returns(false);

            // Setup BookingHub
            _bookingHubMock
                .Setup(x => x.HoldTable(It.IsAny<BookingHubResponse>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bookingTableService.HoldTable(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.TableId, result.TableId);
            Assert.Equal(accountId, result.AccountId);
            Assert.True(result.IsHeld);
            Assert.True(result.HoldExpiry > DateTimeOffset.Now);
            Assert.Equal(request.Date.Date, result.Date.Date);
            Assert.Equal(request.Time, result.Time);

            // Verify BookingHub was called
            _bookingHubMock.Verify(x => x.HoldTable(It.IsAny<BookingHubResponse>()), Times.Once);

            // Verify cache operations
            _memoryCacheMock.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.AtLeastOnce());

            // Verify cache entry properties
            memoryCacheEntryMock.VerifySet(x => x.AbsoluteExpirationRelativeToNow = It.IsAny<TimeSpan?>());
            memoryCacheEntryMock.VerifyGet(x => x.PostEvictionCallbacks);
            memoryCacheEntryMock.VerifySet(x => x.Value = It.IsAny<object>(), Times.AtLeastOnce());

            // Verify TryGetValue was called
            _memoryCacheMock.Verify(x => x.TryGetValue(It.IsAny<object>(), out expectedValue), Times.AtLeastOnce());
        }
        [Fact]
        public async Task HoldTable_WhenTableNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var request = new TablesRequest
            {
                BarId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                Date = DateTime.Now.Date,
                Time = new TimeSpan(14, 0, 0)
            };

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() =>
                _bookingTableService.HoldTable(request));
        }

        [Fact]
        public async Task HoldTable_WhenInvalidTime_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new TablesRequest
            {
                BarId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                Date = DateTime.Now.Date,
                Time = new TimeSpan(23, 0, 0)
            };

            var table = new Table
            {
                TableId = request.TableId,
                TableName = "Test Table",
                TableType = new TableType { BarId = request.BarId }
            };

            var barTimes = new List<BarTime>
            {
                new BarTime
                {
                    BarId = request.BarId,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0)
                }
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { table });

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            // Setup memory cache mock
            var memoryCacheEntryMock = new Mock<ICacheEntry>();
            var cacheDict = new Dictionary<Guid, TableHoldInfo>();

            _memoryCacheMock
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(memoryCacheEntryMock.Object);

            _memoryCacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Returns(false);

            // Cách mock GetOrCreate đúng
            object expectedValue = new Dictionary<Guid, TableHoldInfo>();
            _memoryCacheMock
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out expectedValue))
                .Returns(false);

            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(memoryCacheEntryMock.Object);

            memoryCacheEntryMock
                .Setup(e => e.Value)
                .Returns(new Dictionary<Guid, TableHoldInfo>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(() =>
                _bookingTableService.HoldTable(request));

            Assert.Equal("Thời gian phải nằm trong giờ mở cửa và giờ đóng cửa của quán Bar!", exception.Message);
        }

        [Fact]
        public async Task HoldTable_WhenBarTimeNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new TablesRequest
            {
                BarId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                Date = DateTime.Now.Date,
                Time = new TimeSpan(14, 0, 0)
            };

            var table = new Table
            {
                TableId = request.TableId,
                TableName = "Test Table",
                TableType = new TableType { BarId = request.BarId }
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { table });

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<BarTime>());

            // Setup memory cache mock
            var memoryCacheEntryMock = new Mock<ICacheEntry>();
            var cacheDict = new Dictionary<Guid, TableHoldInfo>();

            _memoryCacheMock
                .Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(memoryCacheEntryMock.Object);

            _memoryCacheMock
                .Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Returns(false);

            // Cách mock GetOrCreate đúng
            object expectedValue = new Dictionary<Guid, TableHoldInfo>();
            _memoryCacheMock
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out expectedValue))
                .Returns(false);

            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(memoryCacheEntryMock.Object);

            memoryCacheEntryMock
                .Setup(e => e.Value)
                .Returns(new Dictionary<Guid, TableHoldInfo>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() =>
                _bookingTableService.HoldTable(request));

            Assert.Equal("Không tìm thấy khung giờ trong quán bar!", exception.Message);
        }

        [Fact]
        public async Task HoldTable_WhenTableAlreadyHeld_ShouldThrowInvalidDataException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var otherAccountId = Guid.NewGuid();
            var request = new TablesRequest
            {
                BarId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                Date = DateTime.Now.Date,
                Time = new TimeSpan(14, 0, 0)
            };

            var table = new Table
            {
                TableId = request.TableId,
                TableName = "Test Table",
                TableType = new TableType { BarId = request.BarId }
            };

            var barTimes = new List<BarTime>
    {
        new BarTime
        {
            BarId = request.BarId,
            DayOfWeek = (int)request.Date.DayOfWeek,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(22, 0, 0)
        }
    };

            // Setup cache data
            var cacheKey = $"{request.BarId}_{request.TableId}_{request.Date.Date.Date}_{request.Time}";
            var existingHoldInfo = new Dictionary<Guid, TableHoldInfo>
    {
        {
            request.TableId,
            new TableHoldInfo
            {
                AccountId = otherAccountId,
                TableId = request.TableId,
                IsHeld = true,
                HoldExpiry = DateTimeOffset.Now.AddMinutes(5),
                Date = request.Date,
                Time = request.Time
            }
        }
    };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { table });

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            // Setup memory cache mock
            var memoryCacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(memoryCacheEntryMock.Object);

            object expectedValue = existingHoldInfo;
            _memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out expectedValue))
                .Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(() =>
                _bookingTableService.HoldTable(request));

            Assert.Contains($"Bàn {request.TableId} đã bị giữ bởi người khác", exception.Message);
        }

        [Fact]
        public async Task HoldTable_WhenTableNotHeld_ShouldCreateNewHold()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new TablesRequest
            {
                BarId = Guid.NewGuid(),
                TableId = Guid.NewGuid(),
                Date = DateTime.Now.Date,
                Time = new TimeSpan(14, 0, 0)
            };

            var table = new Table
            {
                TableId = request.TableId,
                TableName = "Test Table",
                TableType = new TableType { BarId = request.BarId }
            };

            var barTimes = new List<BarTime>
    {
        new BarTime
        {
            BarId = request.BarId,
            DayOfWeek = (int)request.Date.DayOfWeek,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(22, 0, 0)
        }
    };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { table });

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            // Setup Memory Cache
            var memoryCacheEntryMock = new Mock<ICacheEntry>();
            var cacheDict = new Dictionary<Guid, TableHoldInfo>();

            // Setup Value và các property cho ICacheEntry
            memoryCacheEntryMock.SetupProperty(e => e.Value, cacheDict);
            memoryCacheEntryMock.SetupProperty(e => e.AbsoluteExpiration);
            memoryCacheEntryMock.SetupProperty(e => e.AbsoluteExpirationRelativeToNow);
            memoryCacheEntryMock.SetupProperty(e => e.Priority);
            memoryCacheEntryMock.SetupProperty(e => e.Size);
            memoryCacheEntryMock.SetupProperty(e => e.SlidingExpiration);

            memoryCacheEntryMock
                .SetupGet(x => x.ExpirationTokens)
                .Returns(new List<IChangeToken>());

            memoryCacheEntryMock
                .SetupGet(x => x.PostEvictionCallbacks)
                .Returns(new List<PostEvictionCallbackRegistration>());

            // Setup IMemoryCache
            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(memoryCacheEntryMock.Object);

            object expectedValue = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out expectedValue))
                .Returns(false);

            // Setup BookingHub
            _bookingHubMock
                .Setup(x => x.HoldTable(It.IsAny<BookingHubResponse>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bookingTableService.HoldTable(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.TableId, result.TableId);
            Assert.Equal(accountId, result.AccountId);
            Assert.True(result.IsHeld);
            Assert.True(result.HoldExpiry > DateTimeOffset.Now);
            Assert.Equal(request.Date.Date, result.Date.Date);
            Assert.Equal(request.Time, result.Time);

            // Verify BookingHub was called
            _bookingHubMock.Verify(x => x.HoldTable(It.IsAny<BookingHubResponse>()), Times.Once);

            // Verify cache operations
            _memoryCacheMock.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.AtLeastOnce());

            // Verify cache entry properties
            memoryCacheEntryMock.VerifySet(x => x.AbsoluteExpirationRelativeToNow = It.IsAny<TimeSpan?>());
            memoryCacheEntryMock.VerifyGet(x => x.PostEvictionCallbacks);
            memoryCacheEntryMock.VerifySet(x => x.Value = It.IsAny<object>(), Times.AtLeastOnce());
        }

        [Fact]
        public async Task HoldTableList_WhenValidRequest_ShouldReturnListOfTableHoldInfo()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var request = new DateTimeRequest
            {
                Date = DateTime.Now.Date,
                Time = new TimeSpan(14, 0, 0)
            };

            var barTimes = new List<BarTime>
            {
                new BarTime
                {
                    BarId = barId,
                    DayOfWeek = (int)request.Date.DayOfWeek,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0)
                }
            };

            var tables = new List<Table>
            {
                new Table
                {
                    TableId = Guid.NewGuid(),
                    TableName = "Table 1",
                    TableType = new TableType { BarId = barId }
                },
                new Table
                {
                    TableId = Guid.NewGuid(),
                    TableName = "Table 2",
                    TableType = new TableType { BarId = barId }
                }
            };

            var existingHoldInfos = new Dictionary<Guid, TableHoldInfo>
            {
                {
                    tables[0].TableId,
                    new TableHoldInfo
                    {
                        TableId = tables[0].TableId,
                        AccountId = Guid.NewGuid(),
                        IsHeld = true,
                        HoldExpiry = DateTimeOffset.Now.AddMinutes(5),
                        Date = request.Date,
                        Time = request.Time
                    }
                }
            };

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(tables);

            object cacheValue = existingHoldInfos;
            _memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(true);

            // Act
            var result = await _bookingTableService.HoldTableList(barId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal(tables[0].TableId, result[0].TableId);
            Assert.True(result[0].IsHeld);
        }

        [Fact]
        public async Task HoldTableList_WhenNoTablesFound_ShouldReturnEmptyList()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var request = new DateTimeRequest
            {
                Date = DateTime.Now.Date,
                Time = new TimeSpan(14, 0, 0)
            };

            var barTimes = new List<BarTime>
    {
        new BarTime
        {
            BarId = barId,
            DayOfWeek = (int)request.Date.DayOfWeek,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(22, 0, 0)
        }
    };

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.Get(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(barTimes);

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table>());

            object cacheValue = new Dictionary<Guid, TableHoldInfo>();
            _memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheValue))
                .Returns(true);

            // Act
            var result = await _bookingTableService.HoldTableList(barId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
