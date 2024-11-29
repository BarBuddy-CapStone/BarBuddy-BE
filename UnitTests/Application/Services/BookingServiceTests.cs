using Application.DTOs.Booking;
using Application.DTOs.Payment;
using Application.Interfaces;
using Application.IService;
using Application.Service;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Application.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IAuthentication> _authenticationMock;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IQRCodeService> _qrCodeServiceMock;
        private readonly Mock<IFirebase> _firebaseMock;
        private readonly Mock<IEventVoucherService> _eventVoucherServiceMock;
        private readonly Mock<IGenericRepository<Booking>> _bookingRepoMock;
        private readonly Mock<IGenericRepository<Account>> _accountRepoMock;
        private readonly Mock<IGenericRepository<Bar>> _barRepoMock;
        private readonly Mock<IGenericRepository<BarTime>> _barTimeRepoMock;
        private readonly BookingService _bookingService;
        private readonly Mock<IHttpContextAccessor> _contextAccessor;
        private readonly Mock<IFcmService> _fcmService;
        public BookingServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _authenticationMock = new Mock<IAuthentication>();
            _paymentServiceMock = new Mock<IPaymentService>();
            _emailSenderMock = new Mock<IEmailSender>();
            _notificationServiceMock = new Mock<INotificationService>();
            _qrCodeServiceMock = new Mock<IQRCodeService>();
            _firebaseMock = new Mock<IFirebase>();
            _eventVoucherServiceMock = new Mock<IEventVoucherService>();
            _contextAccessor = new Mock<IHttpContextAccessor>();
            _fcmService = new Mock<IFcmService>();

            _bookingRepoMock = new Mock<IGenericRepository<Booking>>();
            _accountRepoMock = new Mock<IGenericRepository<Account>>();
            _barRepoMock = new Mock<IGenericRepository<Bar>>();
            _barTimeRepoMock = new Mock<IGenericRepository<BarTime>>();

            _unitOfWorkMock.Setup(x => x.BookingRepository).Returns(_bookingRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.AccountRepository).Returns(_accountRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.BarRepository).Returns(_barRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.BarTimeRepository).Returns(_barTimeRepoMock.Object);

            _bookingService = new BookingService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _authenticationMock.Object,
                _paymentServiceMock.Object,
                _emailSenderMock.Object,
                _notificationServiceMock.Object,
                _qrCodeServiceMock.Object,
                _firebaseMock.Object,
                _eventVoucherServiceMock.Object,
                _contextAccessor.Object,
                _fcmService.Object                
            );
        }

        // Tests cho CancelBooking
        [Fact]
        public async Task CancelBooking_WhenValidBooking_ShouldReturnTrue()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };
            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = accountId,
                Status = 0, // Pending
                BookingDate = DateTime.UtcNow.AddDays(1),
                BookingTime = TimeSpan.FromHours(14),
                Bar = new Bar { BarName = "Test Bar" }
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking> { booking });

            // Act
            var result = await _bookingService.CancelBooking(bookingId);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(x => x.BookingRepository.UpdateAsync(It.IsAny<Booking>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelBooking_WhenBookingNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _bookingService.CancelBooking(bookingId));
        }

        [Fact]
        public async Task CancelBooking_WhenBookingTimeLessThan2Hours_ShouldReturnFalse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };
            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = accountId,
                Status = 0,
                BookingDate = DateTime.Now,
                BookingTime = DateTime.Now.TimeOfDay,
                Bar = new Bar { BarName = "Test Bar" }
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking> { booking });

            // Act
            var result = await _bookingService.CancelBooking(bookingId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CancelBooking_WhenBookingAlreadyCancelled_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };
            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = accountId,
                Status = 1, // Đã hủy
                BookingDate = DateTime.UtcNow.AddDays(1),
                BookingTime = TimeSpan.FromHours(14),
                Bar = new Bar { BarName = "Test Bar" }
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking> { booking });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _bookingService.CancelBooking(bookingId));
        }

        // Tests cho GetAllCustomerBooking
        [Fact]
        public async Task GetAllCustomerBooking_WhenValidRequest_ShouldReturnBookings()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var customerId = accountId; // CustomerId phải bằng accountId để pass quyền truy cập
            var bookingId1 = Guid.NewGuid();
            var barId1 = Guid.NewGuid();
            var barId2 = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId }; // Tạo account để mock

            var bookings = new List<Booking>
            {
                new Booking
                {
                    BookingId = bookingId1,
                    Status = 0,
                    BarId = barId1,
                    Bar = new Bar { BarName = "Test Bar 1", Images = "image1.jpg,image2.jpg" }
                },
                new Booking
                {
                    BookingId = Guid.NewGuid(),
                    Status = 0,
                    BarId = barId2,
                    Bar = new Bar { BarName = "Test Bar 2", Images = "image3.jpg,image4.jpg" }
                }
            };

            var feedbacks = new List<Feedback>
            {
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    BookingId = bookingId1,
                    Comment = "Great service!",
                    CreatedTime = DateTime.Now,
                    IsDeleted = false
                }
            };

            // Mock authentication để trả về accountId
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            // Mock account repository để trả về account
            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bookings);

            _unitOfWorkMock.Setup(x => x.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(feedbacks);

            // Act
            var (result, totalPage) = await _bookingService.GetAllCustomerBooking(customerId, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, totalPage);
        }

        [Fact]
        public async Task GetListBookingAuthorized_WhenValidRequest_ShouldReturnFilteredBookings()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, BarName = "Test Bar" };
            var booking = new Booking
            {
                BookingId = bookingId,
                Account = new Account
                {
                    Fullname = "Test User",
                    Phone = "1234567890",
                    Email = "test@email.com"
                },
                BarId = barId,
                BookingDate = DateTime.Now.Date,
                BookingTime = TimeSpan.FromHours(14),
                Status = 0
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByIdAsync(barId))
                .ReturnsAsync(bar);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking> { booking });

            // Act
            var result = await _bookingService.GetListBookingAuthorized(
                qrTicket: string.Empty,
                BarId: barId,
                CustomerName: "Test User",
                Phone: "1234567890",
                Email: "test@email.com",
                bookingDate: DateTime.Now.Date,
                bookingTime: TimeSpan.FromHours(14),
                Status: 0,
                1,
                10);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Item1);
            Assert.Equal(1, result.Item2);
        }

        [Fact]
        public async Task GetListBookingAuthorized_WhenUnauthorized_ShouldThrowUnAuthorizedException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = differentBarId };
            var bar = new Bar { BarId = barId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByIdAsync(barId))
                .ReturnsAsync(bar);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(() =>
                _bookingService.GetListBookingAuthorized(
                    qrTicket: string.Empty,
                    BarId: barId,
                    CustomerName: null,
                    Phone: null,
                    Email: null,
                    bookingDate: null,
                    bookingTime: null,
                    Status: null,
                    1,
                    10));
        }

        [Fact]
        public async Task GetListBookingAuthorized_WhenBarNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByIdAsync(barId))
                .ReturnsAsync((Bar)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() =>
                _bookingService.GetListBookingAuthorized(
                    qrTicket: string.Empty,
                    BarId: barId,
                    CustomerName: null,
                    Phone: null,
                    Email: null,
                    bookingDate: null,
                    bookingTime: null,
                    Status: null,
                    1,
                    10));
        }

        [Fact]
        public async Task GetListBookingAuthorized_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId };
            var bookings = new List<Booking>();
            
            // Create 15 bookings for testing pagination
            for (int i = 0; i < 15; i++)
            {
                bookings.Add(new Booking
                {
                    BookingId = Guid.NewGuid(),
                    Account = new Account { Fullname = $"User {i}" },
                    BarId = barId
                });
            }

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByIdAsync(barId))
                .ReturnsAsync(bar);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetListBookingAuthorized(
                qrTicket: string.Empty,
                BarId: barId,
                CustomerName: null,
                Phone: null,
                Email: null,
                bookingDate: null,
                bookingTime: null,
                Status: null,
                2, // Page 2
                10); // 10 items per page

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Item2); // Should have 2 pages (15 items with 10 per page)
        }

        [Fact]
        public async Task GetTopBookingByCustomer_WhenValidRequest_ShouldReturnTopBookings()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var customerId = accountId; // CustomerId phải bằng accountId để pass quyền truy cập
            var bookingId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };
            var bookings = new List<Booking>
            {
                new Booking
                {
                    BookingId = bookingId,
                    AccountId = accountId,
                    Status = 3,
                    Bar = new Bar
                    {
                        BarName = "Test Bar",
                        Images = "image1.jpg,image2.jpg"
                    },
                    BookingDate = DateTime.Now,
                    BookingTime = TimeSpan.FromHours(14),
                    CreateAt = DateTime.Now,
                }
            };
            var feedbacks = new List<Feedback>
            {
                new Feedback
                {
                FeedbackId = Guid.NewGuid(),
                BookingId = bookingId,
                Comment = "Great service!",
                CreatedTime = DateTime.Now,
                IsDeleted = false
                }
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bookings);

            _unitOfWorkMock.Setup(x => x.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(feedbacks);

            // Act
            var result = await _bookingService.GetTopBookingByCustomer(customerId, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Bar", result[0].BarName);
            Assert.Equal("image1.jpg", result[0].Image);
            Assert.True(result[0].IsRated); // Vì có feedback
        }

        [Fact]
        public async Task GetTopBookingByCustomer_WhenUnauthorized_ShouldThrowUnAuthorizedException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var differentCustomerId = Guid.NewGuid(); // CustomerId khác với accountId
            var account = new Account { AccountId = accountId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _bookingService.GetTopBookingByCustomer(differentCustomerId, 0));
        }

        [Fact]
        public async Task GetTopBookingByCustomer_WhenNoBookings_ShouldReturnEmptyList()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var customerId = accountId;
            var account = new Account { AccountId = accountId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _bookingService.GetTopBookingByCustomer(customerId, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTopBookingByCustomer_WhenBookingWithoutFeedback_ShouldReturnIsRatedFalse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var customerId = accountId;
            var bookingId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };
            var bookings = new List<Booking>
            {
                new Booking
                {
                    BookingId = bookingId,
                    AccountId = accountId,
                    Status = 3, // Completed status
                    Bar = new Bar
                    {
                        BarName = "Test Bar",
                        Images = "image1.jpg,image2.jpg"
                    },
                    BookingDate = DateTime.Now,
                    BookingTime = TimeSpan.FromHours(14),
                    CreateAt = DateTime.Now
                }
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(x => x.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bookings);

            _unitOfWorkMock.Setup(x => x.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Feedback>());

            // Act
            var result = await _bookingService.GetTopBookingByCustomer(customerId, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.False(result[0].IsRated); // Không có feedback nên IsRated = false
        }

        [Fact]
        public async Task CreateBookingTableOnly_WhenValidRequest_ShouldReturnBookingResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var request = new BookingTableRequest
            {
                BarId = barId,
                BookingDate = DateTime.Now.Date.AddDays(1),
                BookingTime = TimeSpan.FromHours(14),
                TableIds = new List<Guid> { tableId }
            };

            var account = new Account { AccountId = accountId, Fullname = "Test User" };
            var bar = new Bar { BarId = barId, BarName = "Test Bar" };
            var barTimes = new List<BarTime>
            {
                new BarTime
                {
                    BarId = barId,
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(22)
                }
            };
            var table = new Table { TableId = tableId };
            var booking = new Booking
            {
                BookingId = bookingId,
                Account = account,
                Bar = bar,
                BookingDate = request.BookingDate,
                BookingTime = request.BookingTime
            };
            var expectedResponse = new BookingResponse
            {
                BookingId = bookingId,
                BarName = bar.BarName
            };

            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            var validBase64QrCode = Convert.ToBase64String(Encoding.UTF8.GetBytes("Test QR Code"));


            // Setup mocks
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByID(barId))
                .Returns(bar);

            _barTimeRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(barTimes);

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { table });

            _mapperMock.Setup(x => x.Map<Booking>(It.IsAny<BookingTableRequest>()))
                .Returns(booking);

            _mapperMock.Setup(x => x.Map<BookingResponse>(It.IsAny<Booking>()))
                .Returns(expectedResponse);

            _qrCodeServiceMock.Setup(x => x.GenerateQRCode(It.IsAny<Guid>()))
                .Returns(validBase64QrCode);

            _firebaseMock.Setup(x => x.UploadImageAsync(file))
                .ReturnsAsync("QR_CODE_URL");

            // Act
            var result = await _bookingService.CreateBookingTableOnly(request, new DefaultHttpContext());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.BookingId, result.BookingId);
            Assert.Equal(expectedResponse.BarName, result.BarName);

            _unitOfWorkMock.Verify(x => x.BookingRepository.Insert(It.IsAny<Booking>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
        }
        [Fact]
        public async Task CreateBookingTableOnly_WhenAccountNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var request = new BookingTableRequest();
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(Guid.NewGuid());
            _accountRepoMock.Setup(x => x.GetByID(It.IsAny<Guid>()))
                .Returns((Account)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() =>
                _bookingService.CreateBookingTableOnly(request, new DefaultHttpContext()));
        }

        // Tests cho UpdateBookingStatus
        [Fact]
        public async Task UpdateBookingStatus_WhenValidRequest_ShouldUpdateSuccessfully()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var tableId = Guid.NewGuid();

            var account = new Account { AccountId = accountId, BarId = barId };
            var booking = new Booking
            {
                BookingId = bookingId,
                BarId = barId,
                Status = 0,
                BookingDate = DateTime.Now.Date
            };
            var bookingTables = new List<BookingTable>
            {
                new BookingTable
                {
                    BookingId = bookingId,
                    TableId = tableId
                }
            };
            var table = new Table { TableId = tableId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _bookingRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Booking> { booking });

            _unitOfWorkMock.Setup(x => x.BookingTableRepository.Get(
                It.IsAny<Expression<Func<BookingTable, bool>>>(),
                It.IsAny<Func<IQueryable<BookingTable>, IOrderedQueryable<BookingTable>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(bookingTables);

            _unitOfWorkMock.Setup(x => x.TableRepository.GetByID(tableId))
                .Returns(table);

            // Act
            await _bookingService.UpdateBookingStatus(bookingId, 2, null);

            // Assert
            Assert.Equal(2, booking.Status);
            Assert.Equal(1, table.Status);
            _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        [Fact]
        public async Task UpdateBookingStatus_WhenBookingNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var account = new Account { AccountId = accountId, BarId = barId };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _bookingRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Booking>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _bookingService.UpdateBookingStatus(bookingId, 2, null));
        }

        [Fact]
        public async Task UpdateBookingStatus_WhenBarIdNotCorrect_ShouldThrowDataUnAuthorizedException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var account = new Account { AccountId = accountId, BarId = barId };
            var booking = new Booking { BookingId = bookingId, Account = account, BarId = Guid.NewGuid() };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _bookingRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Booking> { booking });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _bookingService.UpdateBookingStatus(bookingId, 2, null));
        }

        [Fact]
        public async Task UpdateBookingStatus_WhenCancelledBooking_ShouldThrowInvalidDataException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var booking = new Booking
            {
                BookingId = bookingId,
                BarId = barId,
                Status = 1, // Cancelled
                BookingDate = DateTime.Now.Date
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _bookingRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Booking> { booking });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _bookingService.UpdateBookingStatus(bookingId, 2, null));
        }

        [Fact]
        public async Task UpdateBookingStatus_WhenInvalidAdditionalFee_ShouldThrowInvalidDataException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var booking = new Booking
            {
                BookingId = bookingId,
                BarId = barId,
                Status = 0,
                BookingDate = DateTime.Now.Date
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _bookingRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Booking> { booking });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _bookingService.UpdateBookingStatus(bookingId, 3, -100));
        }

        [Fact]
        public async Task CreateBookingTableWithDrinks_WhenValidRequest_ShouldReturnPaymentLink()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var account = new Account { AccountId = accountId };
            var bar = new Bar 
            { 
                BarId = barId, 
                BarName = "Test Bar",
                Discount = 10,
                BarTimes = new List<BarTime>
                {
                    new BarTime
                    {
                        StartTime = TimeSpan.FromHours(10),
                        EndTime = TimeSpan.FromHours(18)
                    }
                }
            };

            var request = new BookingDrinkRequest
            {
                BarId = barId,
                BookingDate = DateTime.Now.Date.AddDays(1),
                BookingTime = TimeSpan.FromHours(14),
                TableIds = new List<Guid> { tableId },
                Drinks = new List<DrinkRequest>
                {
                    new DrinkRequest
                    {
                        DrinkId = drinkId,
                        Quantity = 2
                    }
                },
                PaymentDestination = "MOMO"
            };

            var drink = new Drink { DrinkId = drinkId, Price = 100000, BarId = barId };
            var table = new Table { TableId = tableId, TableType = new TableType { BarId = barId } };
            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = accountId,
                BarId = barId,
                Bar = bar,
                BookingTables = new List<BookingTable>(),
                BookingDrinks = new List<BookingDrink>()
            };

            var paymentLink = new PaymentLink { PaymentUrl = "test-payment-url" };
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");
            var validBase64QrCode = Convert.ToBase64String(Encoding.UTF8.GetBytes("Test QR Code"));

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByID(barId))
                .Returns(bar);

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.GetAsync(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bar.BarTimes);

            _unitOfWorkMock.Setup(x => x.DrinkRepository.Get(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Drink> { drink });

            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { table });

            _mapperMock.Setup(x => x.Map<Booking>(It.IsAny<BookingDrinkRequest>()))
                .Returns(booking);

            _paymentServiceMock.Setup(x => x.GetPaymentLink(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<bool>()))
                .Returns(paymentLink);

            _qrCodeServiceMock.Setup(x => x.GenerateQRCode(It.IsAny<Guid>()))
                .Returns(validBase64QrCode);

            _firebaseMock.Setup(x => x.UploadImageAsync(file))
                .ReturnsAsync("QR_CODE_URL");

            // Act
            var result = await _bookingService.CreateBookingTableWithDrinks(request, new DefaultHttpContext());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(paymentLink.PaymentUrl, result.PaymentUrl);
            _unitOfWorkMock.Verify(x => x.BookingRepository.Insert(It.IsAny<Booking>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        [Fact]
        public async Task CreateBookingTableWithDrinks_WhenInvalidAccount_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new BookingDrinkRequest();

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns((Account)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _bookingService.CreateBookingTableWithDrinks(request, new DefaultHttpContext()));
        }

        [Fact]
        public async Task CreateBookingTableWithDrinks_WhenInvalidBar_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };
            
            var request = new BookingDrinkRequest
            {
                BarId = barId
            };

            // Tạo một booking object để mock mapper
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                AccountId = accountId,
                BarId = barId,
                BookingTables = new List<BookingTable>(),
                BookingDrinks = new List<BookingDrink>()
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByID(barId))
                .Returns((Bar)null);

            // Thêm mock cho mapper
            _mapperMock.Setup(x => x.Map<Booking>(It.IsAny<BookingDrinkRequest>()))
                .Returns(booking);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _bookingService.CreateBookingTableWithDrinks(request, new DefaultHttpContext()));
        }

        [Fact]
        public async Task CreateBookingTableWithDrinks_WhenNoBarTimes_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };
            var bar = new Bar { BarId = barId, BarName = "Test Bar" };
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                AccountId = accountId,
                BarId = barId,
                Bar = bar,
                BookingTables = new List<BookingTable>(),
                BookingDrinks = new List<BookingDrink>()
            };
            var request = new BookingDrinkRequest
            {
                BarId = barId
            };

            _mapperMock.Setup(x => x.Map<Booking>(It.IsAny<BookingDrinkRequest>()))
                .Returns(booking);
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByID(barId))
                .Returns(bar);

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.GetAsync(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<BarTime>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _bookingService.CreateBookingTableWithDrinks(request, new DefaultHttpContext()));
        }

        [Fact]
        public async Task CreateBookingTableWithDrinks_WhenInvalidDrinks_ShouldThrowInvalidDataException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };
            var bar = new Bar 
            { 
                BarId = barId,
                BarTimes = new List<BarTime>
                {
                    new BarTime
                    {
                        StartTime = TimeSpan.FromHours(10),
                        EndTime = TimeSpan.FromHours(18)
                    }
                }
            };

            var request = new BookingDrinkRequest
            {
                BarId = barId,
                BookingDate = DateTime.Now.Date.AddDays(1),
                BookingTime = TimeSpan.FromHours(14),
                Drinks = new List<DrinkRequest>
                {
                    new DrinkRequest { DrinkId = drinkId }
                }
            };

            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                AccountId = accountId,
                BarId = barId,
                Bar = bar,
                BookingTables = new List<BookingTable>(),
                BookingDrinks = new List<BookingDrink>()
            };

            var validBase64QrCode = Convert.ToBase64String(Encoding.UTF8.GetBytes("Test QR Code"));
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            _barRepoMock.Setup(x => x.GetByID(barId))
                .Returns(bar);

            _mapperMock.Setup(x => x.Map<Booking>(It.IsAny<BookingDrinkRequest>()))
                .Returns(booking);

            // Mock cho QR code và Firebase
            _qrCodeServiceMock.Setup(x => x.GenerateQRCode(It.IsAny<Guid>()))
                .Returns(validBase64QrCode);

            _firebaseMock.Setup(x => x.UploadImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("QR_CODE_URL");

            _unitOfWorkMock.Setup(x => x.BarTimeRepository.GetAsync(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bar.BarTimes);

            _unitOfWorkMock.Setup(x => x.DrinkRepository.Get(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Drink>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _bookingService.CreateBookingTableWithDrinks(request, new DefaultHttpContext()));
        }

        [Fact]
        public async Task CreateBookingTableWithDrinks_WhenInvalidTable_ShouldThrowInvalidDataException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var request = new BookingDrinkRequest
            {
                BarId = barId,
                BookingDate = DateTime.Now.Date.AddDays(1),
                BookingTime = TimeSpan.FromHours(14),
                TableIds = new List<Guid> { tableId },
                Drinks = new List<DrinkRequest>
                {
                    new DrinkRequest
                    {
                        DrinkId = drinkId,
                        Quantity = 2
                    }
                },
                PaymentDestination = "MOMO"
            };

            var account = new Account { AccountId = accountId };
            var bar = new Bar
            {
                BarId = barId,
                BarName = "Test Bar",
                Discount = 10,
                BarTimes = new List<BarTime>
                {
                    new BarTime
                    {
                        StartTime = TimeSpan.FromHours(10),
                        EndTime = TimeSpan.FromHours(18)
                    }
                }
            };
            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = accountId,
                BarId = barId,
                Bar = bar,
                BookingTables = new List<BookingTable>(),
                BookingDrinks = new List<BookingDrink>()
            };
            var drink = new Drink { DrinkId = drinkId, Price = 100000, BarId = barId };
            var paymentLink = new PaymentLink { PaymentUrl = "test-payment-url" };
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            var validBase64QrCode = Convert.ToBase64String(Encoding.UTF8.GetBytes("Test QR Code"));

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);
            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);
            _barRepoMock.Setup(x => x.GetByID(barId))
                .Returns(bar);
            _unitOfWorkMock.Setup(x => x.BarTimeRepository.GetAsync(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bar.BarTimes);
            _unitOfWorkMock.Setup(x => x.DrinkRepository.Get(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Drink> { drink });
            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table>());
            _mapperMock.Setup(x => x.Map<Booking>(It.IsAny<BookingDrinkRequest>()))
                .Returns(booking);
            _paymentServiceMock.Setup(x => x.GetPaymentLink(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<bool>()))
                .Returns(paymentLink);
            _qrCodeServiceMock.Setup(x => x.GenerateQRCode(It.IsAny<Guid>()))
                .Returns(validBase64QrCode);
            _firebaseMock.Setup(x => x.UploadImageAsync(file))
                .ReturnsAsync("QR_CODE_URL");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(() =>
                    _bookingService.CreateBookingTableWithDrinks(request, new DefaultHttpContext()));
            Assert.Equal("Some TableIds do not exist.", exception.Message);
        }

        [Fact]
        public async Task CreateBookingTableWithDrinks_WhenTableRequestEmpty_ShouldThrowInvalidDataException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableId = Guid.NewGuid();
            var drinkId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var request = new BookingDrinkRequest
            {
                BarId = barId,
                BookingDate = DateTime.Now.Date.AddDays(1),
                BookingTime = TimeSpan.FromHours(14),
                TableIds = new List<Guid>(),
                Drinks = new List<DrinkRequest>
                {
                    new DrinkRequest
                    {
                        DrinkId = drinkId,
                        Quantity = 2
                    }
                },
                PaymentDestination = "MOMO"
            };

            var account = new Account { AccountId = accountId };
            var bar = new Bar
            {
                BarId = barId,
                BarName = "Test Bar",
                Discount = 10,
                BarTimes = new List<BarTime>
                {
                    new BarTime
                    {
                        StartTime = TimeSpan.FromHours(10),
                        EndTime = TimeSpan.FromHours(18)
                    }
                }
            };
            var table = new Table { TableId = tableId };
            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = accountId,
                BarId = barId,
                Bar = bar,
                BookingTables = new List<BookingTable>(),
                BookingDrinks = new List<BookingDrink>()
            };
            var drink = new Drink { DrinkId = drinkId, Price = 100000 };

            var paymentLink = new PaymentLink { PaymentUrl = "test-payment-url" };
            var bytes = Encoding.UTF8.GetBytes("This is a dummy file");
            IFormFile file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Data", "dummy.txt");

            var validBase64QrCode = Convert.ToBase64String(Encoding.UTF8.GetBytes("Test QR Code"));

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);
            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);
            _barRepoMock.Setup(x => x.GetByID(barId))
                .Returns(bar);
            _unitOfWorkMock.Setup(x => x.BarTimeRepository.GetAsync(
                It.IsAny<Expression<Func<BarTime, bool>>>(),
                It.IsAny<Func<IQueryable<BarTime>, IOrderedQueryable<BarTime>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bar.BarTimes);
            _unitOfWorkMock.Setup(x => x.DrinkRepository.Get(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Drink>());
            _unitOfWorkMock.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table>());
            _mapperMock.Setup(x => x.Map<Booking>(It.IsAny<BookingDrinkRequest>()))
                .Returns(booking);
            _paymentServiceMock.Setup(x => x.GetPaymentLink(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<bool>()))
                .Returns(paymentLink);
            _qrCodeServiceMock.Setup(x => x.GenerateQRCode(It.IsAny<Guid>()))
                .Returns(validBase64QrCode);
            _firebaseMock.Setup(x => x.UploadImageAsync(file))
                .ReturnsAsync("QR_CODE_URL");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(() =>
                    _bookingService.CreateBookingTableWithDrinks(request, new DefaultHttpContext()));
            Assert.Equal("Booking request does not have table field", exception.Message);
        }

        [Fact]
        public async Task GetAllBookingByStsPending_WhenHasBookings_ShouldReturnBookingList()
        {
            // Arrange
            var now = DateTimeOffset.Now;
            var bookings = new List<Booking>
    {
        new Booking
        {
            BookingId = Guid.NewGuid(),
            BookingDate = now.Date,
            BookingTime = now.TimeOfDay.Add(TimeSpan.FromHours(1)),
            Status = (int)PrefixValueEnum.PendingBooking,
            Bar = new Bar { BarName = "Test Bar" }
        }
    };
            var expectedResponse = new List<BookingCustomResponse>
    {
        new BookingCustomResponse
        {
            BookingId = bookings[0].BookingId,
            BarName = bookings[0].Bar.BarName
        }
    };

            _bookingRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(bookings);
            _mapperMock.Setup(x => x.Map<List<BookingCustomResponse>>(It.IsAny<List<Booking>>()))
                .Returns(expectedResponse);

            // Act
            var result = await _bookingService.GetAllBookingByStsPending();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedResponse[0].BookingId, result[0].BookingId);
        }

        [Fact]
        public async Task GetAllBookingByStsPending_WhenNoBookings_ShouldReturnEmptyList()
        {
            // Arrange
            _bookingRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _bookingService.GetAllBookingByStsPending();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
