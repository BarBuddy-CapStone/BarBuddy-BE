using Application.DTOs.Events.EventVoucher;
using Application.Service;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Moq;
using System.Linq.Expressions;
using static Domain.CustomException.CustomException;

namespace UnitTests.Application.Services
{
    public class EventVoucherServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly EventVoucherService _eventVoucherService;

        public EventVoucherServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _eventVoucherService = new EventVoucherService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateEventVoucher_WhenVoucherNotExist_ShouldCreateSuccessfully()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new EventVoucherRequest 
            { 
                EventVoucherName = "Test Voucher",
                VoucherCode = "TEST123"
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                          .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EventVoucher>().AsQueryable());

            var mappedVoucher = new EventVoucher
            {
                EventVoucherName = request.EventVoucherName,
                VoucherCode = request.VoucherCode
            };

            _mockMapper.Setup(x => x.Map<EventVoucher>(request))
                      .Returns(mappedVoucher);

            // Act
            await _eventVoucherService.CreateEventVoucher(eventId, request);

            // Assert
            mockVoucherRepo.Verify(x => x.InsertAsync(It.IsAny<EventVoucher>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateEventVoucher_WhenVoucherExists_ShouldThrowException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new EventVoucherRequest
            {
                EventVoucherName = "Existing Voucher",
                VoucherCode = "EXIST123"
            };

            var existingVoucher = new EventVoucher
            {
                EventVoucherName = request.EventVoucherName,
                VoucherCode = request.VoucherCode,
                Status = PrefixKeyConstant.TRUE
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                          .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                          .Returns(new List<EventVoucher> { existingVoucher }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventVoucherService.CreateEventVoucher(eventId, request)
            );

            mockVoucherRepo.Verify(x => x.InsertAsync(It.IsAny<EventVoucher>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateEventVoucher_WhenErrorOccurs_ShouldThrowInternalServerError()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var request = new EventVoucherRequest
            {
                EventVoucherName = "Test Voucher",
                VoucherCode = "TEST123"
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                          .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventVoucherService.CreateEventVoucher(eventId, request)
            );
        }

        [Fact]
        public async Task UpdateEventVoucher_WhenVoucherExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var voucherId = Guid.NewGuid();
            var request = new UpdateEventVoucherRequest
            {
                EventVoucherId = voucherId,
                EventVoucherName = "Updated Voucher",
                VoucherCode = "UPDATE123",
                Quantity = 10,
                Discount = 20
            };

            var existingVoucher = new EventVoucher
            {
                EventVoucherId = voucherId,
                EventVoucherName = "Old Voucher",
                VoucherCode = "OLD123",
                Quantity = 5,
                Discount = 10
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(voucherId))
                           .ReturnsAsync(existingVoucher);

            _mockMapper.Setup(x => x.Map(request, existingVoucher))
                       .Returns(existingVoucher);

            // Act
            await _eventVoucherService.UpdateEventVoucher(eventId, request);

            // Assert
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateEventVoucher_WhenVoucherNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var voucherId = Guid.NewGuid();
            var request = new UpdateEventVoucherRequest
            {
                EventVoucherId = voucherId,
                EventVoucherName = "Updated Voucher",
                VoucherCode = "UPDATE123"
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(voucherId))
                           .ReturnsAsync((EventVoucher)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _eventVoucherService.UpdateEventVoucher(eventId, request)
            );

            Assert.Equal("Voucher không tồn tại !", exception.Message);
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateEventVoucher_WhenErrorOccurs_ShouldThrowInternalServerError()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var voucherId = Guid.NewGuid();
            var request = new UpdateEventVoucherRequest
            {
                EventVoucherId = voucherId,
                EventVoucherName = "Updated Voucher",
                VoucherCode = "UPDATE123"
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(voucherId))
                           .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventVoucherService.UpdateEventVoucher(eventId, request)
            );

            Assert.Equal("Lỗi hệ thống !", exception.Message);
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteEventVoucher_WhenVoucherExists_ShouldDeleteSuccessfully()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var eventVoucherId = Guid.NewGuid();

            var existingVoucher = new EventVoucher
            {
                EventVoucherId = eventVoucherId,
                Event = new Event { EventId = eventId }
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EventVoucher> { existingVoucher }.AsQueryable());

            // Act
            await _eventVoucherService.DeleteEventVoucher(eventId, eventVoucherId);

            // Assert
            mockVoucherRepo.Verify(x => x.DeleteAsync(eventVoucherId), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteEventVoucher_WhenVoucherNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var eventVoucherId = Guid.NewGuid();

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EventVoucher>().AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _eventVoucherService.DeleteEventVoucher(eventId, eventVoucherId)
            );

            Assert.Equal("Không tìm thấy voucher !", exception.Message);
            mockVoucherRepo.Verify(x => x.DeleteAsync(eventVoucherId), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteEventVoucher_WhenErrorOccurs_ShouldThrowInternalServerError()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var eventVoucherId = Guid.NewGuid();

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventVoucherService.DeleteEventVoucher(eventId, eventVoucherId)
            );

            Assert.Equal("Lỗi hệ thống !", exception.Message);
            mockVoucherRepo.Verify(x => x.DeleteAsync(eventVoucherId), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetVoucherBasedEventId_WhenVoucherExists_ShouldReturnVoucher()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var voucherId = Guid.NewGuid();

            var expectedVoucher = new EventVoucher
            {
                EventVoucherId = voucherId,
                EventId = eventId,
                EventVoucherName = "Test Voucher",
                VoucherCode = "TEST123",
                Status = PrefixKeyConstant.TRUE,
                Quantity = 10,
                Discount = 20
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<EventVoucher> { expectedVoucher }.AsQueryable());

            // Act
            var result = await _eventVoucherService.GetVoucherBasedEventId(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedVoucher.EventVoucherId, result.EventVoucherId);
            Assert.Equal(expectedVoucher.EventId, result.EventId);
            Assert.Equal(expectedVoucher.EventVoucherName, result.EventVoucherName);
            Assert.Equal(expectedVoucher.VoucherCode, result.VoucherCode);
            Assert.Equal(expectedVoucher.Status, result.Status);
            Assert.Equal(expectedVoucher.Quantity, result.Quantity);
            Assert.Equal(expectedVoucher.Discount, result.Discount);
        }

        [Fact]
        public async Task GetVoucherBasedEventId_WhenVoucherNotFound_ShouldReturnNull()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<EventVoucher>().AsQueryable());

            // Act
            var result = await _eventVoucherService.GetVoucherBasedEventId(eventId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetVoucherBasedEventId_WhenErrorOccurs_ShouldThrowInternalServerError()
        {
            // Arrange
            var eventId = Guid.NewGuid();

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventVoucherService.GetVoucherBasedEventId(eventId)
            );

            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }

        [Fact]
        public async Task GetVoucherByCode_WhenValidVoucher_ShouldReturnVoucherResponse()
        {
            // Arrange
            var request = new VoucherQueryRequest
            {
                voucherCode = "TEST123",
                barId = Guid.NewGuid(),
                bookingDate = DateTime.Now,
                bookingTime = TimeSpan.FromHours(19)
            };

            var eventVoucher = new EventVoucher
            {
                EventVoucherId = Guid.NewGuid(),
                VoucherCode = request.voucherCode,
                Quantity = 10,
                Status = PrefixKeyConstant.TRUE,
                Event = new Event
                {
                    Bar = new Bar { BarId = request.barId },
                    TimeEvent = new List<TimeEvent>
                    {
                        new TimeEvent 
                        { 
                            DayOfWeek = (int)request.bookingDate.DayOfWeek,
                            StartTime = TimeSpan.FromHours(18),
                            EndTime = TimeSpan.FromHours(22)
                        }
                    }
                }
            };

            var expectedResponse = new EventVoucherResponse
            {
                EventVoucherId = eventVoucher.EventVoucherId,
                VoucherCode = eventVoucher.VoucherCode
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EventVoucher> { eventVoucher }.AsQueryable());

            _mockMapper.Setup(x => x.Map<EventVoucherResponse>(eventVoucher))
                       .Returns(expectedResponse);

            // Act
            var result = await _eventVoucherService.GetVoucherByCode(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.EventVoucherId, result.EventVoucherId);
            Assert.Equal(expectedResponse.VoucherCode, result.VoucherCode);
        }

        [Fact]
        public async Task GetVoucherByCode_WhenVoucherNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var request = new VoucherQueryRequest
            {
                voucherCode = "NOTFOUND123",
                barId = Guid.NewGuid(),
                bookingDate = DateTime.Now,
                bookingTime = TimeSpan.FromHours(19)
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EventVoucher>().AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _eventVoucherService.GetVoucherByCode(request)
            );

            Assert.Equal("Không tìm thấy voucher", exception.Message);
        }

        [Fact]
        public async Task GetVoucherByCode_WhenInvalidBookingTime_ShouldThrowInvalidDataException()
        {
            // Arrange
            var request = new VoucherQueryRequest
            {
                voucherCode = "TEST123",
                barId = Guid.NewGuid(),
                bookingDate = DateTime.Now,
                bookingTime = TimeSpan.FromHours(23)
            };

            var eventVoucher = new EventVoucher
            {
                VoucherCode = request.voucherCode,
                Quantity = 10,
                Status = PrefixKeyConstant.TRUE,
                Event = new Event
                {
                    Bar = new Bar { BarId = request.barId },
                    TimeEvent = new List<TimeEvent>
                    {
                        new TimeEvent 
                        { 
                            DayOfWeek = (int)request.bookingDate.DayOfWeek,
                            StartTime = TimeSpan.FromHours(18),
                            EndTime = TimeSpan.FromHours(22)
                        }
                    }
                }
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EventVoucher> { eventVoucher }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _eventVoucherService.GetVoucherByCode(request)
            );
        }

        [Fact]
        public async Task GetVoucherByCode_WhenErrorOccurs_ShouldThrowInternalServerError()
        {
            // Arrange
            var request = new VoucherQueryRequest
            {
                voucherCode = "TEST123",
                barId = Guid.NewGuid(),
                bookingDate = DateTime.Now,
                bookingTime = TimeSpan.FromHours(19)
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.Get(
                It.IsAny<Expression<Func<EventVoucher, bool>>>(),
                It.IsAny<Func<IQueryable<EventVoucher>, IOrderedQueryable<EventVoucher>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventVoucherService.GetVoucherByCode(request)
            );

            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }

        [Fact]
        public async Task UpdateStatusVoucher_WhenVoucherExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var eventVoucherId = Guid.NewGuid();
            var existingVoucher = new EventVoucher
            {
                EventVoucherId = eventVoucherId,
                EventVoucherName = "Test Voucher",
                VoucherCode = "TEST123",
                Status = PrefixKeyConstant.TRUE
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(eventVoucherId))
                           .ReturnsAsync(existingVoucher);

            // Act
            await _eventVoucherService.UpdateStatusVoucher(eventVoucherId);

            // Assert
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusVoucher_WhenVoucherNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var eventVoucherId = Guid.NewGuid();

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(eventVoucherId))
                           .ReturnsAsync((EventVoucher)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _eventVoucherService.UpdateStatusVoucher(eventVoucherId)
            );

            Assert.Equal("Không tìm thấy voucher !", exception.Message);
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusVoucher_WhenErrorOccurs_ShouldThrowInternalServerError()
        {
            // Arrange
            var eventVoucherId = Guid.NewGuid();

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(eventVoucherId))
                           .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventVoucherService.UpdateStatusVoucher(eventVoucherId)
            );

            Assert.Equal("Lỗi hệ thống !", exception.Message);
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusNStsVoucher_WhenVoucherExists_ShouldUpdateSuccessfully()
        {
            // Arrange
            var eventVoucherId = Guid.NewGuid();
            var quantityVoucher = 5;
            var status = true;

            var existingVoucher = new EventVoucher
            {
                EventVoucherId = eventVoucherId,
                EventVoucherName = "Test Voucher",
                VoucherCode = "TEST123",
                Status = false,
                Quantity = 10
            };

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(eventVoucherId))
                           .ReturnsAsync(existingVoucher);

            // Act
            await _eventVoucherService.UpdateStatusNStsVoucher(eventVoucherId, quantityVoucher, status);

            // Assert
            Assert.Equal(status, existingVoucher.Status);
            Assert.Equal(quantityVoucher, existingVoucher.Quantity);
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStatusNStsVoucher_WhenVoucherNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var eventVoucherId = Guid.NewGuid();
            var quantityVoucher = 5;
            var status = true;

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(eventVoucherId))
                           .ReturnsAsync((EventVoucher)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _eventVoucherService.UpdateStatusNStsVoucher(eventVoucherId, quantityVoucher, status)
            );

            Assert.Equal("Không tìm thấy voucher !", exception.Message);
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusNStsVoucher_WhenErrorOccurs_ShouldThrowInternalServerError()
        {
            // Arrange
            var eventVoucherId = Guid.NewGuid();
            var quantityVoucher = 5;
            var status = true;

            var mockVoucherRepo = new Mock<IGenericRepository<EventVoucher>>();
            _mockUnitOfWork.Setup(x => x.EventVoucherRepository)
                           .Returns(mockVoucherRepo.Object);

            mockVoucherRepo.Setup(x => x.GetByIdAsync(eventVoucherId))
                           .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _eventVoucherService.UpdateStatusNStsVoucher(eventVoucherId, quantityVoucher, status)
            );

            Assert.Equal("Lỗi hệ thống !", exception.Message);
            mockVoucherRepo.Verify(x => x.UpdateRangeAsync(It.IsAny<EventVoucher>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }
    }
}
