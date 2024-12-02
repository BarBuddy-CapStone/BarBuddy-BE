using Application.DTOs.NotificationDetails;
using Application.Interfaces;
using Application.IService;
using Application.Service;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Application.Services
{
    public class NotificationDetailServiceTest
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IAuthentication> _authenticationMock;
        private readonly NotificationDetailService _service;

        public NotificationDetailServiceTest()
        {
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _authenticationMock = new Mock<IAuthentication>();

            _service = new NotificationDetailService(
                _mapperMock.Object,
                _unitOfWorkMock.Object,
                _httpContextAccessorMock.Object,
                _authenticationMock.Object
            );
        }

        [Fact]
        public async Task CreateNotificationDetail_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new NotificationDetailRequest { AccountId = userId };
            var notificationDetail = new NotificationDetail();

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _mapperMock.Setup(x => x.Map<NotificationDetail>(request))
                .Returns(notificationDetail);

            _unitOfWorkMock.Setup(x => x.NotificationDetailRepository.InsertAsync(It.IsAny<NotificationDetail>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateNotificationDetail(request);

            // Assert
            Assert.True(result);
            Assert.False(notificationDetail.IsRead);
            _unitOfWorkMock.Verify(x => x.NotificationDetailRepository.InsertAsync(notificationDetail), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateNotificationDetail_InvalidUserId_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new NotificationDetailRequest { AccountId = Guid.NewGuid() };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(() => 
                _service.CreateNotificationDetail(request));
        }

        [Fact]
        public async Task CreateNotificationDetail_RepositoryError_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new NotificationDetailRequest { AccountId = userId };
            var notificationDetail = new NotificationDetail();

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _mapperMock.Setup(x => x.Map<NotificationDetail>(request))
                .Returns(notificationDetail);

            _unitOfWorkMock.Setup(x => x.NotificationDetailRepository.InsertAsync(It.IsAny<NotificationDetail>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.CreateNotificationDetail(request);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateNotificationDetailJob_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var request = new NotificationDetailRequest { AccountId = Guid.NewGuid() };
            var notificationDetail = new NotificationDetail();

            _mapperMock.Setup(x => x.Map<NotificationDetail>(request))
                .Returns(notificationDetail);

            _unitOfWorkMock.Setup(x => x.NotificationDetailRepository.InsertAsync(It.IsAny<NotificationDetail>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateNotificationDetailJob(request);

            // Assert
            Assert.True(result);
            Assert.False(notificationDetail.IsRead);
            _unitOfWorkMock.Verify(x => x.NotificationDetailRepository.InsertAsync(notificationDetail), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateNotificationDetailJob_RepositoryError_ReturnsFalse()
        {
            // Arrange
            var request = new NotificationDetailRequest { AccountId = Guid.NewGuid() };
            var notificationDetail = new NotificationDetail();

            _mapperMock.Setup(x => x.Map<NotificationDetail>(request))
                .Returns(notificationDetail);

            _unitOfWorkMock.Setup(x => x.NotificationDetailRepository.InsertAsync(It.IsAny<NotificationDetail>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _service.CreateNotificationDetailJob(request);

            // Assert
            Assert.False(result);
        }
    }
}
