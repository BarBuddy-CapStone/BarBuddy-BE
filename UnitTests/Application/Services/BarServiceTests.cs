using Application.DTOs.Bar;
using Application.DTOs.BarTime;
using Application.Interfaces;
using Application.IService;
using Application.Service;
using AutoMapper;
using Domain.Common;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Domain.CustomException.CustomException;

namespace UnitTests.Application.Services
{
    public class BarServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IFirebase> _firebaseMock;
        private readonly Mock<IBarTimeService> _barTimeServiceMock;
        private readonly Mock<IGenericRepository<Bar>> _barRepoMock;
        private readonly BarService _barService;
        private readonly Mock<IAuthentication> _authenticationMock;
        private readonly Mock<IHttpContextAccessor> _contextAccessorMock;

        public BarServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _firebaseMock = new Mock<IFirebase>();
            _barTimeServiceMock = new Mock<IBarTimeService>();
            _barRepoMock = new Mock<IGenericRepository<Bar>>();
            _authenticationMock = new Mock<IAuthentication>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _barService = new BarService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _firebaseMock.Object,
                _barTimeServiceMock.Object,
                _authenticationMock.Object,
                _contextAccessorMock.Object
            );

            _unitOfWorkMock.Setup(x => x.BarRepository).Returns(_barRepoMock.Object);
        }

        [Fact]
        public async Task CreateBar_WhenValidRequest_ShouldCreateBarSuccessfully()
        {
            // Arrange
            var request = new CreateBarRequest
            {
                BarName = "Test Bar",
                Images = new List<string> { "UxdxLxCa2kaTkTCHf4S4uw==" },
                TimeSlot = 1,
                BarTimeRequest = new List<BarTimeRequest>
            {
                new BarTimeRequest
                {
                    DayOfWeek = 1,
                    StartTime = TimeSpan.FromHours(8),
                    EndTime = TimeSpan.FromHours(22)
                }
            }
            };

            var bar = new Bar { BarId = Guid.NewGuid() };
            var formFile = new Mock<IFormFile>();

            _barRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar>());

            _mapperMock.Setup(x => x.Map<Bar>(request))
                .Returns(bar);

            _firebaseMock.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("image-url");

            // Act
            await _barService.CreateBar(request);

            // Assert
            _barRepoMock.Verify(x => x.InsertAsync(It.IsAny<Bar>()), Times.Once);
            _barRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Bar>()), Times.Once);
            _barTimeServiceMock.Verify(x => x.CreateBarTimeOfBar(bar.BarId, request.BarTimeRequest), Times.Once);
        }

        [Fact]
        public async Task CreateBar_WhenBarNameExists_ShouldThrowException()
        {
            // Arrange
            var request = new CreateBarRequest
            {
                BarName = "Existing Bar",
                Images = new List<string> { "base64Image" }
            };

            var existingBar = new Bar { BarName = request.BarName };

            _barRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { existingBar });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                async () => await _barService.CreateBar(request));
        }

        [Fact]
        public async Task GetAllBar_WhenBarsExist_ShouldReturnBarResponses()
        {
            // Arrange
            var query = new ObjectQueryCustom { Search = "Test" };
            var bars = new List<Bar>
        {
            new Bar { BarId = Guid.NewGuid(), BarName = "Test Bar 1" },
            new Bar { BarId = Guid.NewGuid(), BarName = "Test Bar 2" }
        };
            var barResponses = bars.Select(b => new BarResponse { BarId = b.BarId, BarName = b.BarName });

            _barRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bars);

            _mapperMock.Setup(x => x.Map<IEnumerable<BarResponse>>(bars))
                .Returns(barResponses);

            // Act
            var result = await _barService.GetAllBar(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bars.Count, result.BarResponses.Count());
        }

        [Fact]
        public async Task GetAllBar_WhenNoBarsExist_ShouldThrowException()
        {
            // Arrange
            var query = new ObjectQueryCustom();

            _barRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                async () => await _barService.GetAllBar(query));
        }

        [Fact]
        public async Task GetBarById_WhenBarExists_ShouldReturnBarResponse()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var bar = new Bar
            {
                BarId = barId,
                BarName = "Test Bar",
                BarTimes = new List<BarTime>
        {
            new BarTime { DayOfWeek = 1, StartTime = TimeSpan.FromHours(8) }
        }
            };
            var barResponse = new BarResponse
            {
                BarId = barId,
                BarName = "Test Bar",
                BarTimeResponses = new List<BarTimeResponse>()
            };

            _barRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.Is<string>(s => s == "BarTimes"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar> { bar });

            _mapperMock.Setup(x => x.Map<BarResponse>(bar))
                .Returns(barResponse);
            _mapperMock.Setup(x => x.Map<List<BarTimeResponse>>(bar.BarTimes))
                .Returns(new List<BarTimeResponse>());

            // Act
            var result = await _barService.GetBarById(barId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(barId, result.BarId);
        }

        [Fact]
        public async Task GetBarById_WhenBarNotFound_ShouldThrowException()
        {
            // Arrange
            var barId = Guid.NewGuid();

            _barRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                async () => await _barService.GetBarById(barId));
        }

        [Fact]
        public async Task UpdateBarById_WithValidData_ShouldReturnUpdatedBar()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var request = new UpdateBarRequest
            {
                BarId = barId,
                BarName = "Test Bar",
                Images = new List<string> { "tVNytRDxPE+mXdfKwyLlPg==" },
                imgsAsString = new List<string> { "existing.jpg" },
                TimeSlot = 1,
                UpdateBarTimeRequests = new List<UpdateBarTimeRequest>
        {
            new UpdateBarTimeRequest
            {
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(22),
                DayOfWeek = 1
            }
        }
            };

            var existingBar = new Bar
            {
                BarId = barId,
                BarName = "Old Bar Name",
                Images = "old-image.jpg",
                BarTimes = new List<BarTime>()
            };

            // Setup mock cho BarRepository
            _barRepoMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar> { existingBar });

            // Setup mock cho EventRepository
            var eventRepoMock = new Mock<IGenericRepository<Event>>();
            eventRepoMock.Setup(x => x.Get(It.IsAny<Expression<Func<Event, bool>>>(),
                It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Event>());
            _unitOfWorkMock.Setup(x => x.EventRepository).Returns(eventRepoMock.Object);

            // Setup mock cho Firebase
            _firebaseMock.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("new-image-url");

            // Setup mapper
            _mapperMock.Setup(x => x.Map<OnlyBarResponse>(It.IsAny<Bar>()))
                .Returns(new OnlyBarResponse { BarId = barId });
            _mapperMock.Setup(x => x.Map(request, existingBar))
                .Returns(existingBar);

            // Act
            var result = await _barService.UpdateBarById(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(barId, result.BarId);
            _barRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Bar>()), Times.AtLeast(1));
            _barTimeServiceMock.Verify(x => x.UpdateBarTimeOfBar(barId, request.UpdateBarTimeRequests), Times.Once);
        }
        [Fact]
        public async Task UpdateBarById_WithNonExistentBar_ShouldThrowNotFoundException()
        {
            // Arrange
            var request = new UpdateBarRequest { BarId = Guid.NewGuid() };

            _barRepoMock.Setup(x => x.GetAsync(
               It.IsAny<Expression<Func<Bar, bool>>>(),
               It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
               It.Is<string>(s => s == "BarTimes"),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(new List<Bar>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _barService.UpdateBarById(request));
        }

        [Fact]
        public async Task UpdateBarById_WhenBarNotFound_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateBarRequest
            {
                BarId = Guid.NewGuid(),
                BarName = "Non Existing Bar"
            };

            _barRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                async () => await _barService.UpdateBarById(request));
        }

        [Fact]
        public async Task GetAllAvailableBars_WhenBarsExistAndAvailable_ShouldReturnBars()
        {
            // Arrange
            var dateTime = DateTime.Now;
            var query = new ObjectQuery();
            var barId = Guid.NewGuid();

            var bars = new List<Bar>
            {
                new Bar
                {
                    BarId = barId,
                    BarName = "Test Bar 1",
                    BarTimes = new List<BarTime>
                    {
                        new BarTime
                        {
                            DayOfWeek = dateTime.DayOfWeek.GetHashCode(),
                            StartTime = TimeSpan.FromHours(8),
                            EndTime = TimeSpan.FromHours(22)
                        }       
                    }
                }
            };

            var bookingTableRepo = new Mock<IGenericRepository<BookingTable>>();
            var tableRepo = new Mock<IGenericRepository<Table>>();

            _barRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.Is<string>(s => s == "Feedbacks,BarTimes"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bars);

            tableRepo.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table> { new Table { TableId = Guid.NewGuid() } });

            bookingTableRepo.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<BookingTable, bool>>>(),
                It.IsAny<Func<IQueryable<BookingTable>, IOrderedQueryable<BookingTable>>>(),
                It.Is<string>(s => s == "Booking"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<BookingTable>());

            _unitOfWorkMock.Setup(x => x.TableRepository).Returns(tableRepo.Object);
            _unitOfWorkMock.Setup(x => x.BookingTableRepository).Returns(bookingTableRepo.Object);

            _mapperMock.Setup(x => x.Map<OnlyBarResponse>(It.IsAny<Bar>()))
                .Returns(new OnlyBarResponse { BarId = barId });
            _mapperMock.Setup(x => x.Map<List<BarTimeResponse>>(It.IsAny<List<BarTime>>()))
                .Returns(new List<BarTimeResponse>());

            // Act
            var result = await _barService.GetAllAvailableBars(dateTime, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllAvailableBars_WhenSearchByName_ShouldReturnFilteredBars()
        {
            // Arrange
            var dateTime = DateTime.Now;
            var query = new ObjectQuery { Search = "Test" };
            var barId = Guid.NewGuid();
            var bars = new List<Bar>
            {
                new Bar
                {
                    BarId = barId,
                    BarName = "Test Bar",
                    BarTimes = new List<BarTime>
                    {
                        new BarTime
                        {
                            DayOfWeek = dateTime.DayOfWeek.GetHashCode(),
                            StartTime = TimeSpan.FromHours(8),
                            EndTime = TimeSpan.FromHours(22)
                        }
                    },
                    Feedbacks = new List<Feedback>()
                }
            };

            var bookingTableRepo = new Mock<IGenericRepository<BookingTable>>();
            var tableRepo = new Mock<IGenericRepository<Table>>();

            _barRepoMock.Setup(x => x.GetAsync(
                It.Is<Expression<Func<Bar, bool>>>(expr => expr.ToString().Contains("Contains")),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.Is<string>(s => s == "Feedbacks,BarTimes"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bars);

            tableRepo.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table> { new Table { TableId = Guid.NewGuid() } });

            bookingTableRepo.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<BookingTable, bool>>>(),
                It.IsAny<Func<IQueryable<BookingTable>, IOrderedQueryable<BookingTable>>>(),
                It.Is<string>(s => s == "Booking"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<BookingTable>());

            _unitOfWorkMock.Setup(x => x.TableRepository).Returns(tableRepo.Object);
            _unitOfWorkMock.Setup(x => x.BookingTableRepository).Returns(bookingTableRepo.Object);

            _mapperMock.Setup(x => x.Map<OnlyBarResponse>(It.IsAny<Bar>()))
                .Returns(new OnlyBarResponse { BarId = barId, BarName = "Test Bar" });
            _mapperMock.Setup(x => x.Map<List<BarTimeResponse>>(It.IsAny<List<BarTime>>()))
                .Returns(new List<BarTimeResponse>
                {
            new BarTimeResponse
            {
                DayOfWeek = dateTime.DayOfWeek.GetHashCode(),
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(22)
            }
                });

            // Act
            var result = await _barService.GetAllBarWithFeedback(query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, x => x.BarName.Contains("Test"));
        }

        // GetRevenueOfBar - Valid Cases
        [Fact]
        public async Task GetRevenueOfBar_WhenValidDateRange_ShouldReturnRevenue()
        {
            // Arrange
            var request = new RevenueRequest
            {
                FromTime = DateTime.Now.AddDays(-7),
                ToTime = DateTime.Now,
                BarId = Guid.NewGuid().ToString()
            };

            var bar = new Bar { BarId = Guid.Parse(request.BarId) };
            var bookings = new List<Booking>
    {
        new Booking
        {
            BookingDate = DateTime.Now.AddDays(-5),
            TotalPrice = 100,
            AdditionalFee = 10
        }
    };

            _barRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(bar);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _barService.GetRevenueOfBar(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(110, result.RevenueOfBar);
        }

        [Fact]
        public async Task GetRevenueOfBar_WhenNoDateRange_ShouldReturnAllTimeRevenue()
        {
            // Arrange
            var request = new RevenueRequest { BarId = Guid.NewGuid().ToString() };
            var bookings = new List<Booking>
    {
        new Booking { TotalPrice = 100, AdditionalFee = 10 }
    };

            _barRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Bar());

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _barService.GetRevenueOfBar(request);

            // Assert
            Assert.NotNull(result);
        }

        // GetRevenueOfBar - Invalid Cases
        [Fact]
        public async Task GetRevenueOfBar_WhenInvalidDateRange_ShouldThrowException()
        {
            // Arrange
            var request = new RevenueRequest
            {
                FromTime = DateTime.Now,
                ToTime = DateTime.Now.AddDays(-7)
            };

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                async () => await _barService.GetRevenueOfBar(request));
        }

        [Fact]
        public async Task GetRevenueOfBar_WhenBarNotFound_ShouldThrowException()
        {
            // Arrange
            var request = new RevenueRequest
            {
                BarId = Guid.NewGuid().ToString(),
                FromTime = DateTime.Now.AddDays(-7),
                ToTime = DateTime.Now
            };

            _barRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Bar)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                async () => await _barService.GetRevenueOfBar(request));
        }

        [Fact]
        public async Task GetAllRevenueBranch_WhenDataExists_ShouldReturnRevenue()
        {
            // Arrange
            var bookings = new List<Booking>
            {
                new Booking { TotalPrice = 100, AdditionalFee = 10 },
                new Booking { TotalPrice = 200, AdditionalFee = 20 }
            };

            var bars = new List<Bar>
            {
                new Bar { BarId = Guid.NewGuid() },
                new Bar { BarId = Guid.NewGuid() }
            };

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bookings);

            _barRepoMock.Setup(x => x.GetAllAsync())
                .ReturnsAsync(bars);

            // Act
            var result = await _barService.GetAllRevenueBranch();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(330, result.RevenueBranch); // (100+10) + (200+20)
            Assert.Equal(2, result.TotalBarBranch);
            Assert.Equal(2, result.TotalBooking);
        }

        [Fact]
        public async Task GetAllRevenueBranch_WhenError_ShouldThrowException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new InternalServerErrorException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                async () => await _barService.GetAllRevenueBranch());
        }
    }
}
