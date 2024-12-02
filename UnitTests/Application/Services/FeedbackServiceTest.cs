using Application.DTOs.Feedback;
using Application.DTOs.Request.FeedBackRequest;
using Application.DTOs.Response.FeedBack;
using Application.Interfaces;
using Application.Service;
using AutoMapper;
using Domain.Common;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using static Domain.CustomException.CustomException;

namespace UnitTests.Application.Services
{
    public class FeedbackServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IAuthentication> _mockAuthentication;
        private readonly FeedBackService _feedbackService;

        public FeedbackServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockAuthentication = new Mock<IAuthentication>();
            _feedbackService = new FeedBackService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockHttpContextAccessor.Object,
                _mockAuthentication.Object
            );
        }

        [Fact]
        public async Task GetFeedBack_WhenFeedbacksExist_ReturnsListOfFeedbacks()
        {
            // Arrange
            var feedbacks = new List<Feedback>
            {
                new Feedback { 
                    BookingId = Guid.NewGuid(),
                    BarId = Guid.NewGuid(),
                    Comment = "Test 1",
                    Account = new Account { Image = "image1.jpg", Fullname = "User 1" },
                    Bar = new Bar { BarName = "Bar 1" }
                },
                new Feedback { 
                    BookingId = Guid.NewGuid(),
                    BarId = Guid.NewGuid(),
                    Comment = "Test 2",
                    Account = new Account { Image = "image2.jpg", Fullname = "User 2" },
                    Bar = new Bar { BarName = "Bar 2" }
                }
            };

            var expectedResponses = new List<FeedBackResponse>
            {
                new FeedBackResponse { 
                    BookingId = feedbacks[0].BookingId,
                    BarId = feedbacks[0].BarId,
                    Comment = "Test 1",
                    ImageAccount = "image1.jpg",
                    AccountName = "User 1",
                    BarName = "Bar 1",
                    CreatedTime = feedbacks[0].CreatedTime,
                    LastUpdatedTime = feedbacks[0].LastUpdatedTime
                },
                new FeedBackResponse { 
                    BookingId = feedbacks[1].BookingId,
                    BarId = feedbacks[1].BarId,
                    Comment = "Test 2",
                    ImageAccount = "image2.jpg",
                    AccountName = "User 2",
                    BarName = "Bar 2",
                    CreatedTime = feedbacks[1].CreatedTime,
                    LastUpdatedTime = feedbacks[1].LastUpdatedTime
                }
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Get(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(feedbacks.AsQueryable());

            _mockMapper.Setup(m => m.Map<IEnumerable<FeedBackResponse>>(It.IsAny<IEnumerable<Feedback>>()))
                .Returns(expectedResponses);

            // Act
            var result = await _feedbackService.GetFeedBack();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponses.Count, result.Count());
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Get(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s == "Account,Bar"),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task GetFeedBack_WhenNoFeedbacks_ThrowsDataNotFoundException()
        {
            // Arrange
            var emptyFeedbacks = new List<Feedback>().AsQueryable();

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Get(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(emptyFeedbacks);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _feedbackService.GetFeedBack()
            );

            Assert.Equal("No feedback in Database", exception.Message);
        }

        [Fact]
        public async Task GetFeedBack_VerifyMapping()
        {
            // Arrange
            var feedbacks = new List<Feedback>
            {
                new Feedback { 
                    BookingId = Guid.NewGuid(),
                    BarId = Guid.NewGuid(),
                    Comment = "Test",
                    Rating = 5,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now,
                    Account = new Account { Image = "test.jpg", Fullname = "Test User" },
                    Bar = new Bar { BarName = "Test Bar" }
                }
            };

            var expectedResponse = new FeedBackResponse
            {
                BookingId = feedbacks[0].BookingId,
                BarId = feedbacks[0].BarId,
                Comment = "Test",
                Rating = 5,
                CreatedTime = feedbacks[0].CreatedTime,
                LastUpdatedTime = feedbacks[0].LastUpdatedTime,
                ImageAccount = "test.jpg",
                AccountName = "Test User",
                BarName = "Test Bar"
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Get(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(feedbacks.AsQueryable());

            _mockMapper.Setup(m => m.Map<IEnumerable<FeedBackResponse>>(It.IsAny<IEnumerable<Feedback>>()))
                .Returns(new List<FeedBackResponse> { expectedResponse });

            // Act
            var result = await _feedbackService.GetFeedBack();

            // Assert
            var firstResult = result.First();
            Assert.Equal(expectedResponse.BookingId, firstResult.BookingId);
            Assert.Equal(expectedResponse.BarId, firstResult.BarId);
            Assert.Equal(expectedResponse.Comment, firstResult.Comment);
            Assert.Equal(expectedResponse.Rating, firstResult.Rating);
            Assert.Equal(expectedResponse.CreatedTime, firstResult.CreatedTime);
            Assert.Equal(expectedResponse.LastUpdatedTime, firstResult.LastUpdatedTime);
            Assert.Equal(expectedResponse.ImageAccount, firstResult.ImageAccount);
            Assert.Equal(expectedResponse.AccountName, firstResult.AccountName);
            Assert.Equal(expectedResponse.BarName, firstResult.BarName);
        }

        [Fact]
        public async Task GetFeedBackByID_WhenFeedbackExists_ReturnsFeedback()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var feedback = new Feedback
            {
                FeedbackId = feedbackId,
                BookingId = Guid.NewGuid(),
                BarId = Guid.NewGuid(),
                Comment = "Test feedback",
                Rating = 4,
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now,
                Account = new Account { Image = "user.jpg", Fullname = "Test User" },
                Bar = new Bar { BarName = "Test Bar" }
            };

            var expectedResponse = new FeedBackResponse
            {
                BookingId = feedback.BookingId,
                BarId = feedback.BarId,
                Comment = feedback.Comment,
                Rating = feedback.Rating,
                CreatedTime = feedback.CreatedTime,
                LastUpdatedTime = feedback.LastUpdatedTime,
                ImageAccount = "user.jpg",
                AccountName = "Test User",
                BarName = "Test Bar"
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Get(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s == "Account,Bar"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Feedback> { feedback }.AsQueryable());

            _mockMapper.Setup(m => m.Map<FeedBackResponse>(It.IsAny<Feedback>()))
                .Returns(expectedResponse);

            // Act
            var result = await _feedbackService.GetFeedBackByID(feedbackId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.BookingId, result.BookingId);
            Assert.Equal(expectedResponse.BarId, result.BarId);
            Assert.Equal(expectedResponse.Comment, result.Comment);
            Assert.Equal(expectedResponse.Rating, result.Rating);
            Assert.Equal(expectedResponse.CreatedTime, result.CreatedTime);
            Assert.Equal(expectedResponse.LastUpdatedTime, result.LastUpdatedTime);
            Assert.Equal(expectedResponse.ImageAccount, result.ImageAccount);
            Assert.Equal(expectedResponse.AccountName, result.AccountName);
            Assert.Equal(expectedResponse.BarName, result.BarName);

            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Get(
                It.Is<Expression<Func<Feedback, bool>>>(expr => expr.Compile()(feedback)),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s == "Account,Bar"),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task GetFeedBackByID_WhenFeedbackDoesNotExist_ThrowsDataNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Get(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s == "Account,Bar"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Feedback>().AsQueryable());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _feedbackService.GetFeedBackByID(nonExistentId)
            );

            Assert.Equal("Không tìm thấy Feedback.", exception.Message);
        }

        [Fact]
        public async Task GetFeedBackByID_VerifyRepositoryCall()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var feedback = new Feedback
            {
                FeedbackId = feedbackId,
                BookingId = Guid.NewGuid(),
                Account = new Account(),
                Bar = new Bar()
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Get(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s == "Account,Bar"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Feedback> { feedback }.AsQueryable());

            _mockMapper.Setup(m => m.Map<FeedBackResponse>(It.IsAny<Feedback>()))
                .Returns(new FeedBackResponse());

            // Act
            await _feedbackService.GetFeedBackByID(feedbackId);

            // Assert
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Get(
                It.Is<Expression<Func<Feedback, bool>>>(expr => expr.Compile()(feedback)),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s == "Account,Bar"),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task CreateFeedBack_WhenValidRequest_ReturnsFeedbackResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var request = new CreateFeedBackRequest
            {
                BookingId = bookingId,
                Comment = "Great service!",
                Rating = 5
            };

            var bar = new Bar
            {
                BarId = barId,
                BarName = "Test Bar"
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = userId,
                BarId = barId,
                Status = 3,
                Bar = bar
            };

            var account = new Account 
            { 
                AccountId = userId,
                Fullname = "Test User",
                Image = "user.jpg"
            };

            var expectedResponse = new FeedBackResponse
            {
                BookingId = bookingId,
                BarId = barId,
                Comment = "Great service!",
                Rating = 5,
                IsDeleted = false,
                AccountName = account.Fullname,
                ImageAccount = account.Image,
                BarName = bar.BarName,
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now
            };

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            // Mock AccountRepository
            _mockUnitOfWork.Setup(u => u.AccountRepository.Get(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Account> { account }.AsQueryable());

            // Mock BookingRepository với include Bar
            _mockUnitOfWork.Setup(u => u.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking> { booking });

            // Mock FeedbackRepository
            _mockUnitOfWork.Setup(u => u.FeedbackRepository.InsertAsync(It.IsAny<Feedback>()))
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(m => m.Map<FeedBackResponse>(It.IsAny<Feedback>()))
                .Returns(expectedResponse);

            // Act
            var result = await _feedbackService.CreateFeedBack(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.BookingId, result.BookingId);
            Assert.Equal(expectedResponse.BarId, result.BarId);
            Assert.Equal(expectedResponse.Comment, result.Comment);
            Assert.Equal(expectedResponse.Rating, result.Rating);
            Assert.Equal(expectedResponse.AccountName, result.AccountName);
            Assert.Equal(expectedResponse.ImageAccount, result.ImageAccount);
            Assert.Equal(expectedResponse.BarName, result.BarName);
            Assert.False(result.IsDeleted);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.InsertAsync(
                It.Is<Feedback>(f => 
                    f.BookingId == bookingId &&
                    f.AccountId == userId &&
                    f.BarId == barId &&
                    f.Comment == request.Comment &&
                    f.Rating == request.Rating &&
                    !f.IsDeleted &&
                    f.CreatedTime != default &&
                    f.LastUpdatedTime != default
                )), Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);

            // Verify other dependencies were called
            _mockAuthentication.Verify(a => a.GetUserIdFromHttpContext(httpContext), Times.Once);
            _mockUnitOfWork.Verify(u => u.AccountRepository.Get(
                It.Is<Expression<Func<Account, bool>>>(expr => expr.Compile()(account)),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task CreateFeedBack_WhenBookingNotFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateFeedBackRequest
            {
                BookingId = Guid.NewGuid(),
                Comment = "Test comment",
                Rating = 5
            };

            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            var account = new Account { AccountId = userId };
            _mockUnitOfWork.Setup(u => u.AccountRepository.Get(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Account> { account }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _feedbackService.CreateFeedBack(request)
            );

            Assert.Equal("Không tìm thấy booking", exception.Message);
        }

        [Fact]
        public async Task CreateFeedBack_WhenBookingNotCompleted_ThrowsInvalidDataException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var request = new CreateFeedBackRequest
            {
                BookingId = bookingId,
                Comment = "Test comment",
                Rating = 5
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = userId,
                BarId = barId,
                Status = 1 // Chưa hoàn thành
            };

            var user = new Account 
            { 
                AccountId = userId,
                Fullname = "Test User",
                Image = "test.jpg"
            };

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            // Mock AccountRepository.Get
            _mockUnitOfWork.Setup(u => u.AccountRepository.Get(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Account> { user }.AsQueryable());

            // Mock BookingRepository.GetAsync
            _mockUnitOfWork.Setup(u => u.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking> { booking });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _feedbackService.CreateFeedBack(request)
            );

            Assert.Equal("Không thể đánh giá, lịch đặt bàn chưa hoàn thành", exception.Message);

            // Verify that necessary methods were called
            _mockAuthentication.Verify(a => a.GetUserIdFromHttpContext(httpContext), Times.Once);
            _mockUnitOfWork.Verify(u => u.AccountRepository.Get(
                It.Is<Expression<Func<Account, bool>>>(expr => expr.Compile()(user)),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.BookingRepository.GetAsync(
                It.Is<Expression<Func<Booking, bool>>>(expr => 
                    expr.Compile()(booking) && 
                    booking.Status != 3),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task CreateFeedBack_VerifyFeedbackCreationTime()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            var request = new CreateFeedBackRequest
            {
                BookingId = bookingId,
                Comment = "Test",
                Rating = 5
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = userId,
                BarId = barId,
                Status = 3
            };

            var user = new Account 
            { 
                AccountId = userId,
                Fullname = "Test User",
                Image = "test.jpg"
            };

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            // Mock AccountRepository.Get
            _mockUnitOfWork.Setup(u => u.AccountRepository.Get(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Account> { user }.AsQueryable());

            _mockUnitOfWork.Setup(u => u.BookingRepository.GetAsync(
                It.IsAny<Expression<Func<Booking, bool>>>(),
                It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Booking> { booking });

            Feedback capturedFeedback = null;
            _mockUnitOfWork.Setup(u => u.FeedbackRepository.InsertAsync(It.IsAny<Feedback>()))
                .Callback<Feedback>(f => capturedFeedback = f);

            // Act
            await _feedbackService.CreateFeedBack(request);

            // Assert
            Assert.NotNull(capturedFeedback);
            Assert.Equal(capturedFeedback.CreatedTime, capturedFeedback.LastUpdatedTime);
            Assert.True((DateTimeOffset.Now - capturedFeedback.CreatedTime).TotalSeconds < 5);
            
            // Verify AccountRepository.Get was called with correct userId
            _mockUnitOfWork.Verify(u => u.AccountRepository.Get(
                It.Is<Expression<Func<Account, bool>>>(expr => expr.Compile()(user)),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task UpdateFeedBack_WhenFeedbackExists_ReturnsFeedbackResponse()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            var request = new UpdateFeedBackRequest
            {
                Comment = "Updated comment",
                Rating = 4
            };

            var existingFeedback = new Feedback
            {
                FeedbackId = feedbackId,
                AccountId = userId,
                BarId = barId,
                Comment = "Original comment",
                Rating = 5,
                Account = new Account { Fullname = "Test User", Image = "user.jpg" },
                Bar = new Bar { BarName = "Test Bar" }
            };

            var updatedFeedback = new Feedback
            {
                FeedbackId = feedbackId,
                AccountId = userId,
                BarId = barId,
                Comment = request.Comment,
                Rating = request.Rating,
                Account = existingFeedback.Account,
                Bar = existingFeedback.Bar
            };

            var expectedResponse = new FeedBackResponse
            {
                Comment = request.Comment,
                Rating = request.Rating,
                AccountName = existingFeedback.Account.Fullname,
                ImageAccount = existingFeedback.Account.Image,
                BarName = existingFeedback.Bar.BarName
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByID(feedbackId))
                .Returns(existingFeedback);

            _mockMapper.Setup(m => m.Map(request, existingFeedback))
                .Returns(updatedFeedback);

            _mockMapper.Setup(m => m.Map<FeedBackResponse>(updatedFeedback))
                .Returns(expectedResponse);

            // Act
            var result = await _feedbackService.UpdateFeedBack(feedbackId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Comment, result.Comment);
            Assert.Equal(request.Rating, result.Rating);
            Assert.Equal(existingFeedback.Account.Fullname, result.AccountName);
            Assert.Equal(existingFeedback.Account.Image, result.ImageAccount);
            Assert.Equal(existingFeedback.Bar.BarName, result.BarName);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetByID(feedbackId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Update(updatedFeedback), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateFeedBack_WhenFeedbackDoesNotExist_ThrowsDataNotFoundException()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = new UpdateFeedBackRequest
            {
                Comment = "Updated comment",
                Rating = 4
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByID(feedbackId))
                .Returns((Feedback)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _feedbackService.UpdateFeedBack(feedbackId, request)
            );

            Assert.Equal("Không tìm thấy Feedback.", exception.Message);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetByID(feedbackId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Update(It.IsAny<Feedback>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateFeedBack_VerifyMappingAndUpdate()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var request = new UpdateFeedBackRequest
            {
                Comment = "Updated comment",
                Rating = 4
            };

            var existingFeedback = new Feedback
            {
                FeedbackId = feedbackId,
                Comment = "Original comment",
                Rating = 5,
                CreatedTime = DateTimeOffset.Now.AddHours(-1),
                LastUpdatedTime = DateTimeOffset.Now.AddHours(-1)
            };

            var updatedFeedback = new Feedback
            {
                FeedbackId = feedbackId,
                Comment = request.Comment,
                Rating = request.Rating,
                CreatedTime = DateTimeOffset.Now.AddHours(-1),
                LastUpdatedTime = DateTimeOffset.Now.AddHours(-1)
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByID(feedbackId))
                .Returns(existingFeedback);

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Update(It.IsAny<Feedback>()))
                .Callback<Feedback>(f => updatedFeedback = f);

            _mockMapper.Setup(m => m.Map(request, existingFeedback))
                .Returns(updatedFeedback);

            // Act
            await _feedbackService.UpdateFeedBack(feedbackId, request);

            // Assert
            Assert.NotNull(updatedFeedback);
            Assert.Equal(request.Comment, updatedFeedback.Comment);
            Assert.Equal(request.Rating, updatedFeedback.Rating);
            Assert.Equal(feedbackId, updatedFeedback.FeedbackId);
            Assert.True(updatedFeedback.LastUpdatedTime > existingFeedback.CreatedTime);

            // Verify all mock calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetByID(feedbackId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Update(updatedFeedback), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map(request, existingFeedback), Times.Once);
        }

        [Fact]
        public async Task DeleteUpdateFeedBack_WhenFeedbackExists_ReturnsTrue()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var existingFeedback = new Feedback
            {
                FeedbackId = feedbackId,
                IsDeleted = false,
                Comment = "Test comment",
                Rating = 5,
                CreatedTime = DateTimeOffset.Now.AddHours(-1),
                LastUpdatedTime = DateTimeOffset.Now.AddHours(-1)
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByID(feedbackId))
                .Returns(existingFeedback);

            Feedback updatedFeedback = null;
            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Update(It.IsAny<Feedback>()))
                .Callback<Feedback>(f => updatedFeedback = f);

            // Act
            var result = await _feedbackService.DeleteUpdateFeedBack(feedbackId);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedFeedback);
            Assert.True(updatedFeedback.IsDeleted);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetByID(feedbackId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Update(It.Is<Feedback>(f => 
                f.FeedbackId == feedbackId && 
                f.IsDeleted)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUpdateFeedBack_WhenFeedbackDoesNotExist_ThrowsDataNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByID(nonExistentId))
                .Returns((Feedback)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _feedbackService.DeleteUpdateFeedBack(nonExistentId)
            );

            Assert.Equal("Không tìm thấy.", exception.Message);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetByID(nonExistentId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Update(It.IsAny<Feedback>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteUpdateFeedBack_VerifyUpdateOperation()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var existingFeedback = new Feedback
            {
                FeedbackId = feedbackId,
                IsDeleted = false,
                Comment = "Test comment",
                Rating = 5,
                CreatedTime = DateTimeOffset.Now.AddHours(-1),
                LastUpdatedTime = DateTimeOffset.Now.AddHours(-1)
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByID(feedbackId))
                .Returns(existingFeedback);

            Feedback capturedFeedback = null;
            _mockUnitOfWork.Setup(u => u.FeedbackRepository.Update(It.IsAny<Feedback>()))
                .Callback<Feedback>(f => capturedFeedback = f);

            // Act
            await _feedbackService.DeleteUpdateFeedBack(feedbackId);

            // Assert
            Assert.NotNull(capturedFeedback);
            Assert.Equal(feedbackId, capturedFeedback.FeedbackId);
            Assert.True(capturedFeedback.IsDeleted);
            Assert.Equal(existingFeedback.Comment, capturedFeedback.Comment);
            Assert.Equal(existingFeedback.Rating, capturedFeedback.Rating);
            Assert.Equal(existingFeedback.CreatedTime, capturedFeedback.CreatedTime);

            // Verify the update operation
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.Update(It.Is<Feedback>(f => 
                f.FeedbackId == feedbackId && 
                f.IsDeleted && 
                f.Comment == existingFeedback.Comment && 
                f.Rating == existingFeedback.Rating)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetFeedBackByBookingId_WhenFeedbackExists_ReturnsCustomerFeedbackResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            var account = new Account
            {
                AccountId = userId,
                Fullname = "Test User",
                Image = "user.jpg"
            };

            var bar = new Bar
            {
                BarId = barId,
                BarName = "Test Bar",
                Address = "123 Test Street",
                Images = "bar1.jpg,bar2.jpg"
            };

            var booking = new Booking
            {
                BookingId = bookingId,
                AccountId = userId,
                BarId = barId
            };

            var feedback = new Feedback
            {
                FeedbackId = Guid.NewGuid(),
                BookingId = bookingId,
                AccountId = userId,
                BarId = barId,
                Comment = "Great service!",
                Rating = 5,
                CreatedTime = DateTimeOffset.Now,
                LastUpdatedTime = DateTimeOffset.Now,
                Account = account,
                Bar = bar,
                Booking = booking
            };

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            _mockUnitOfWork.Setup(u => u.AccountRepository.GetByID(userId))
                .Returns(account);

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar,Booking")),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Feedback> { feedback });

            // Act
            var result = await _feedbackService.GetFeedBackByBookingId(bookingId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(feedback.FeedbackId, result.FeedbackId);
            Assert.Equal(bar.Address, result.BarAddress);
            Assert.Equal(bar.Images.Split(',')[0], result.BarImage);
            Assert.Equal(bar.BarName, result.BarName);
            Assert.Equal(feedback.Comment, result.Comment);
            Assert.Equal(feedback.CreatedTime, result.CreatedTime);
            Assert.Equal(account.Image, result.CustomerAvatar);
            Assert.Equal(account.Fullname, result.CustomerName);
            Assert.Equal(feedback.LastUpdatedTime, result.LastUpdatedTime);
            Assert.Equal(feedback.Rating, result.Rating);

            // Verify repository calls
            _mockAuthentication.Verify(a => a.GetUserIdFromHttpContext(httpContext), Times.Once);
            _mockUnitOfWork.Verify(u => u.AccountRepository.GetByID(userId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetAsync(
                It.Is<Expression<Func<Feedback, bool>>>(expr => expr.Compile()(feedback)),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar,Booking")),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task GetFeedBackByBookingId_WhenFeedbackNotFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            var account = new Account
            {
                AccountId = userId,
                Fullname = "Test User",
                Image = "user.jpg"
            };

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            _mockUnitOfWork.Setup(u => u.AccountRepository.GetByID(userId))
                .Returns(account);

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar,Booking")),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Feedback>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _feedbackService.GetFeedBackByBookingId(bookingId)
            );

            Assert.Equal("Đã có lỗi xảy ra, đánh giá không tồn tại", exception.Message);
        }

        [Fact]
        public async Task GetFeedBackByBookingId_WhenExceptionOccurs_ThrowsInternalServerErrorException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookingId = Guid.NewGuid();

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            _mockUnitOfWork.Setup(u => u.AccountRepository.GetByID(userId))
                .Throws(new Exception("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _feedbackService.GetFeedBackByBookingId(bookingId)
            );

            Assert.Equal("Test exception", exception.Message);
        }

        [Fact]
        public async Task GetFeedBackAdmin_WhenFeedbacksExist_ReturnsPaginatedResponse()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 2,
                Search = "Test User"
            };

            var feedbacks = new List<Feedback>
            {
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    BarId = barId,
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now,
                    Account = new Account { Fullname = "Test User 1" },
                    Bar = new Bar { BarName = "Test Bar 1" }
                },
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    BarId = barId,
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now.AddHours(-1),
                    LastUpdatedTime = DateTimeOffset.Now.AddHours(-1),
                    Account = new Account { Fullname = "Test User 2" },
                    Bar = new Bar { BarName = "Test Bar 2" }
                }
            };

            var adminResponses = feedbacks.Select(f => new AdminFeedbackResponse
            {
                FeedbackId = f.FeedbackId,
                CustomerName = f.Account.Fullname,
                BarName = f.Bar.BarName,
                CreatedTime = f.CreatedTime,
                LastUpdatedTime = f.LastUpdatedTime
            }).ToList();

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(feedbacks);

            _mockMapper.Setup(m => m.Map<List<AdminFeedbackResponse>>(It.IsAny<IEnumerable<Feedback>>()))
                .Returns(adminResponses);

            // Act
            var result = await _feedbackService.GetFeedBackAdmin(barId, false, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalItems);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.AdminFeedbackResponses.Count);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetAsync(
                It.Is<Expression<Func<Feedback, bool>>>(expr => 
                    expr.Compile()(feedbacks[0]) && 
                    expr.Compile()(feedbacks[1])),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task GetFeedBackAdmin_WhenNoFeedbacks_ThrowsDataNotFoundException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 10
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Feedback>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _feedbackService.GetFeedBackAdmin(barId, false, query)
            );

            Assert.Equal("Không tìm thấy phản hồi nào!", exception.Message);
        }

        [Fact]
        public async Task GetFeedBackAdmin_WithSearchQuery_FiltersCorrectly()
        {
            // Arrange
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 10,
                Search = "John"
            };

            var feedbacks = new List<Feedback>
            {
                new Feedback
                {
                    Account = new Account { Fullname = "John Doe" },
                    Bar = new Bar { BarName = "Test Bar" }
                }
            };

            var adminResponses = feedbacks.Select(f => new AdminFeedbackResponse
            {
                CustomerName = f.Account.Fullname,
                BarName = f.Bar.BarName
            }).ToList();

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(feedbacks);

            _mockMapper.Setup(m => m.Map<List<AdminFeedbackResponse>>(It.IsAny<IEnumerable<Feedback>>()))
                .Returns(adminResponses);

            // Act
            var result = await _feedbackService.GetFeedBackAdmin(null, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.AdminFeedbackResponses);
            Assert.Contains(result.AdminFeedbackResponses, r => r.CustomerName.Contains("John"));

            // Verify search filter
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetAsync(
                It.Is<Expression<Func<Feedback, bool>>>(expr => 
                    expr.Compile()(feedbacks[0])),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task GetFeedBackAdmin_WithBarIdAndStatus_FiltersCorrectly()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 10
            };

            var feedbacks = new List<Feedback>
            {
                new Feedback
                {
                    BarId = barId,
                    IsDeleted = false,
                    Account = new Account { Fullname = "Test User" },
                    Bar = new Bar { BarName = "Test Bar" }
                }
            };

            var adminResponses = feedbacks.Select(f => new AdminFeedbackResponse
            {
                CustomerName = f.Account.Fullname,
                BarName = f.Bar.BarName
            }).ToList();

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(feedbacks);

            _mockMapper.Setup(m => m.Map<List<AdminFeedbackResponse>>(It.IsAny<IEnumerable<Feedback>>()))
                .Returns(adminResponses);

            // Act
            var result = await _feedbackService.GetFeedBackAdmin(barId, false, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.AdminFeedbackResponses);

            // Verify filters
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetAsync(
                It.Is<Expression<Func<Feedback, bool>>>(expr => 
                    expr.Compile()(feedbacks[0])),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);
        }

        [Fact]
        public async Task UpdateFeedBackByAdmin_WhenFeedbackExists_UpdatesSuccessfully()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var status = true;
            var feedback = new Feedback
            {
                FeedbackId = feedbackId,
                IsDeleted = false
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByIdAsync(feedbackId))
                .ReturnsAsync(feedback);

            Feedback updatedFeedback = null;
            _mockUnitOfWork.Setup(u => u.FeedbackRepository.UpdateAsync(It.IsAny<Feedback>()))
                .Callback<Feedback>(f => updatedFeedback = f)
                .Returns(Task.CompletedTask);

            // Act
            await _feedbackService.UpdateFeedBackByAdmin(feedbackId, status);

            // Assert
            Assert.NotNull(updatedFeedback);
            Assert.Equal(status, updatedFeedback.IsDeleted);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetByIdAsync(feedbackId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.UpdateAsync(It.Is<Feedback>(f => 
                f.FeedbackId == feedbackId && 
                f.IsDeleted == status)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateFeedBackByAdmin_WhenFeedbackDoesNotExist_ThrowsException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var status = true;

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByIdAsync(nonExistentId))
                .ReturnsAsync((Feedback)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InternalServerErrorException>(
                () => _feedbackService.UpdateFeedBackByAdmin(nonExistentId, status)
            );

            Assert.Equal("Không thể tìm thấy đánh giá", exception.Message);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetByIdAsync(nonExistentId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.UpdateAsync(It.IsAny<Feedback>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateFeedBackByAdmin_WhenUpdateFails_ThrowsInternalServerErrorException()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var status = true;
            var feedback = new Feedback
            {
                FeedbackId = feedbackId,
                IsDeleted = false
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByIdAsync(feedbackId))
                .ReturnsAsync(feedback);

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.UpdateAsync(It.IsAny<Feedback>()))
                .ThrowsAsync(new Exception("Update failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _feedbackService.UpdateFeedBackByAdmin(feedbackId, status)
            );

            Assert.Equal("Update failed", exception.Message);

            // Verify repository calls
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetByIdAsync(feedbackId), Times.Once);
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.UpdateAsync(It.IsAny<Feedback>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateFeedBackByAdmin_VerifyStatusUpdate()
        {
            // Arrange
            var feedbackId = Guid.NewGuid();
            var originalStatus = false;
            var newStatus = true;
            
            var feedback = new Feedback
            {
                FeedbackId = feedbackId,
                IsDeleted = originalStatus,
                Comment = "Test comment",
                Rating = 5
            };

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetByIdAsync(feedbackId))
                .ReturnsAsync(feedback);

            Feedback capturedFeedback = null;
            _mockUnitOfWork.Setup(u => u.FeedbackRepository.UpdateAsync(It.IsAny<Feedback>()))
                .Callback<Feedback>(f => capturedFeedback = f)
                .Returns(Task.CompletedTask);

            // Act
            await _feedbackService.UpdateFeedBackByAdmin(feedbackId, newStatus);

            // Assert
            Assert.NotNull(capturedFeedback);
            Assert.Equal(newStatus, capturedFeedback.IsDeleted);
            Assert.Equal(feedback.Comment, capturedFeedback.Comment);
            Assert.Equal(feedback.Rating, capturedFeedback.Rating);
            Assert.Equal(feedbackId, capturedFeedback.FeedbackId);

            // Verify the update operation
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.UpdateAsync(It.Is<Feedback>(f => 
                f.FeedbackId == feedbackId && 
                f.IsDeleted == newStatus && 
                f.Comment == feedback.Comment && 
                f.Rating == feedback.Rating)), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task GetFeedBackManager_WhenAuthorizedAndFeedbacksExist_ReturnsPaginatedResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 2,
                Search = "Test User"
            };

            var account = new Account
            {
                AccountId = userId,
                BarId = barId,
                Fullname = "Manager"
            };

            var feedbacks = new List<Feedback>
            {
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    BarId = barId,
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now,
                    LastUpdatedTime = DateTimeOffset.Now,
                    Account = new Account { Fullname = "Test User 1" },
                    Bar = new Bar { BarName = "Test Bar" }
                },
                new Feedback
                {
                    FeedbackId = Guid.NewGuid(),
                    BarId = barId,
                    IsDeleted = false,
                    CreatedTime = DateTimeOffset.Now.AddHours(-1),
                    LastUpdatedTime = DateTimeOffset.Now.AddHours(-1),
                    Account = new Account { Fullname = "Test User 2" },
                    Bar = new Bar { BarName = "Test Bar" }
                }
            };

            var managerResponses = feedbacks.Select(f => new ManagerFeedbackResponse
            {
                FeedbackId = f.FeedbackId,
                CustomerName = f.Account.Fullname,
                BarName = f.Bar.BarName,
                CreatedTime = f.CreatedTime
            }).ToList();

            // Mock HttpContext
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            _mockUnitOfWork.Setup(u => u.AccountRepository.GetByID(userId))
                .Returns(account);

            // Mock cho lần gọi đầu tiên để kiểm tra tổng số feedback
            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.Is<Expression<Func<Feedback, bool>>>(expr => 
                    expr.Compile()(new Feedback { BarId = barId, IsDeleted = false })),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(feedbacks);

            // Mock cho lần gọi thứ hai với phân trang và search
            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.Is<Expression<Func<Feedback, bool>>>(expr => 
                    expr.Compile()(new Feedback { 
                        BarId = barId, 
                        IsDeleted = false,
                        Account = new Account { Fullname = "Test User" }
                    })),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.Is<int?>(p => p == query.PageIndex),
                It.Is<int?>(p => p == query.PageSize)))
                .ReturnsAsync(feedbacks);

            _mockMapper.Setup(m => m.Map<List<ManagerFeedbackResponse>>(It.IsAny<IEnumerable<Feedback>>()))
                .Returns(managerResponses);

            // Act
            var result = await _feedbackService.GetFeedBackManager(barId, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalItems);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(2, result.ManagerFeedbackResponses.Count);

            // Verify calls
            _mockAuthentication.Verify(a => a.GetUserIdFromHttpContext(httpContext), Times.Once);
            _mockUnitOfWork.Verify(u => u.AccountRepository.GetByID(userId), Times.Once);
            
            // Verify cả hai lần gọi GetAsync
            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetAsync(
                It.Is<Expression<Func<Feedback, bool>>>(expr => 
                    expr.Compile()(new Feedback { BarId = barId, IsDeleted = false })),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()), Times.Once);

            _mockUnitOfWork.Verify(u => u.FeedbackRepository.GetAsync(
                It.Is<Expression<Func<Feedback, bool>>>(expr => 
                    expr.Compile()(new Feedback { 
                        BarId = barId, 
                        IsDeleted = false,
                        Account = new Account { Fullname = "Test User" }
                    })),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.Is<int?>(p => p == query.PageIndex),
                It.Is<int?>(p => p == query.PageSize)), Times.Once);
        }

        [Fact]
        public async Task GetFeedBackManager_WhenUnauthorized_ThrowsUnAuthorizedException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 10
            };

            var account = new Account
            {
                AccountId = userId,
                BarId = differentBarId // Different from requested barId
            };

            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            _mockUnitOfWork.Setup(u => u.AccountRepository.GetByID(userId))
                .Returns(account);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnAuthorizedException>(
                () => _feedbackService.GetFeedBackManager(barId, query)
            );

            Assert.Equal("Bạn không có quyền truy cập vào quán bar này !", exception.Message);
        }

        [Fact]
        public async Task GetFeedBackManager_WhenNoFeedbacks_ThrowsDataNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 10
            };

            var account = new Account
            {
                AccountId = userId,
                BarId = barId
            };

            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContext);

            _mockAuthentication.Setup(a => a.GetUserIdFromHttpContext(httpContext))
                .Returns(userId);

            _mockUnitOfWork.Setup(u => u.AccountRepository.GetByID(userId))
                .Returns(account);

            _mockUnitOfWork.Setup(u => u.FeedbackRepository.GetAsync(
                It.IsAny<Expression<Func<Feedback, bool>>>(),
                It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>(),
                It.Is<string>(s => s.Contains("Account,Bar")),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Feedback>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DataNotFoundException>(
                () => _feedbackService.GetFeedBackManager(barId, query)
            );

            Assert.Equal("Không tìm thấy phản hồi nào!", exception.Message);
        }
    }
}
