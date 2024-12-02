using Application.DTOs.Notification;
using Application.DTOs.NotificationDetails;
using Application.Interfaces;
using Application.IService;
using Application.Service;
using AutoMapper;
using Domain.Constants;
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
    public class NotificationServiceTest
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuthentication> _authenticationMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<INotificationDetailService> _notificationDetailServiceMock;
        private readonly NotificationService _notificationService;

        public NotificationServiceTest()
        {
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _authenticationMock = new Mock<IAuthentication>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _notificationDetailServiceMock = new Mock<INotificationDetailService>();

            _notificationService = new NotificationService(
                _mapperMock.Object,
                _unitOfWorkMock.Object,
                _authenticationMock.Object,
                _httpContextAccessorMock.Object,
                _notificationDetailServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateNotification_Success_ReturnsNotificationResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new NotificationRequest
            {
                Title = "Test Notification",
                Message = "Test Message"
            };

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                Title = request.Title,
                Message = request.Message,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var expectedResponse = new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Message = notification.Message
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _mapperMock.Setup(x => x.Map<Notification>(request))
                .Returns(notification);

            _mapperMock.Setup(x => x.Map<NotificationResponse>(notification))
                .Returns(expectedResponse);

            _unitOfWorkMock.Setup(x => x.NotificationRepository.InsertAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _notificationDetailServiceMock
                .Setup(x => x.CreateNotificationDetail(It.IsAny<NotificationDetailRequest>()));

            // Act
            var result = await _notificationService.CreateNotification(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.NotificationId, result.NotificationId);
            Assert.Equal(expectedResponse.Title, result.Title);
            Assert.Equal(expectedResponse.Message, result.Message);

            _unitOfWorkMock.Verify(x => x.NotificationRepository.InsertAsync(It.IsAny<Notification>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
            _notificationDetailServiceMock.Verify(x => x.CreateNotificationDetail(It.IsAny<NotificationDetailRequest>()), Times.Once);
        }

        [Fact]
        public async Task CreateNotification_WhenExceptionOccurs_ThrowsInternalServerErrorException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new NotificationRequest
            {
                Title = "Test Notification",
                Message = "Test Message"
            };

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                Title = request.Title,
                Message = request.Message,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Mock authentication để lấy userId
            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            // Mock mapper để chuyển đổi request thành notification
            _mapperMock.Setup(x => x.Map<Notification>(request))
                .Returns(notification);

            // Mock repository để throw exception
            _unitOfWorkMock.Setup(x => x.NotificationRepository.InsertAsync(It.IsAny<Notification>()))
                .ThrowsAsync(new Domain.CustomException.CustomException.InternalServerErrorException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Domain.CustomException.CustomException.InternalServerErrorException>(
                () => _notificationService.CreateNotification(request)
            );

            // Verify
            _unitOfWorkMock.Verify(x => x.NotificationRepository.InsertAsync(It.IsAny<Notification>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
            _notificationDetailServiceMock.Verify(x => x.CreateNotificationDetail(It.IsAny<NotificationDetailRequest>()), Times.Never);
        }

        [Fact]
        public async Task UpdateIsReadNoti_Success_ReturnsUpdatedNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateNotiRequest
            {
                AccountId = userId
            };
            var notifications = new List<NotificationDetail>
            {
                new NotificationDetail
                {
                    AccountId = userId,
                    IsRead = PrefixKeyConstant.FALSE,
                    Notification = new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        Title = "Test Notification 1",
                        Message = "Test Message 1",
                        Bar = new Domain.Entities.Bar { BarId = Guid.NewGuid() }
                    }
                },
                new NotificationDetail
                {
                    AccountId = userId,
                    IsRead = PrefixKeyConstant.FALSE,
                    Notification = new Notification
                    {
                        NotificationId = Guid.NewGuid(),
                        Title = "Test Notification 2",
                        Message = "Test Message 2",
                        Bar = new Domain.Entities.Bar { BarId = Guid.NewGuid() }
                    }
                }
            };

            var expectedResponses = notifications.Select(n => new NotificationResponse
            {
                NotificationId = n.Notification.NotificationId,
                Title = n.Notification.Title,
                Message = n.Notification.Message,
                IsRead = PrefixKeyConstant.TRUE
            }).ToList();

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _unitOfWorkMock.Setup(x => x.NotificationDetailRepository.Get(
                    It.IsAny<System.Linq.Expressions.Expression<Func<NotificationDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<NotificationDetail>, IOrderedQueryable<NotificationDetail>>>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .Returns(notifications.AsQueryable());

            _unitOfWorkMock.Setup(x => x.NotificationRepository.UpdateAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.NotificationDetailRepository.UpdateAsync(It.IsAny<NotificationDetail>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _mapperMock.Setup(x => x.Map<List<NotificationResponse>>(It.IsAny<List<NotificationDetail>>()))
                .Returns(expectedResponses);

            // Act
            var result = await _notificationService.UpdateIsReadNoti(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.IsRead == PrefixKeyConstant.TRUE));

            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.NotificationRepository.UpdateAsync(It.IsAny<Notification>()), Times.Exactly(2));
            _unitOfWorkMock.Verify(x => x.NotificationDetailRepository.UpdateAsync(It.IsAny<NotificationDetail>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateIsReadNoti_UnauthorizedUser_ThrowsUnAuthorizedException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var request = new UpdateNotiRequest
            {
                AccountId = differentUserId
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            // Act & Assert
            await Assert.ThrowsAsync<Domain.CustomException.CustomException.UnAuthorizedException>(
                () => _notificationService.UpdateIsReadNoti(request)
            );

            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateIsReadNoti_EmptyNotificationList_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateNotiRequest
            {
                AccountId = userId
            };

            var emptyList = new List<NotificationDetail>();

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _unitOfWorkMock.Setup(x => x.NotificationDetailRepository.Get(
                    It.IsAny<System.Linq.Expressions.Expression<Func<NotificationDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<NotificationDetail>, IOrderedQueryable<NotificationDetail>>>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .Returns(emptyList.AsQueryable());

            _mapperMock.Setup(x => x.Map<List<NotificationResponse>>(It.IsAny<List<NotificationDetail>>()))
                .Returns(new List<NotificationResponse>());

            // Act
            var result = await _notificationService.UpdateIsReadNoti(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
            _unitOfWorkMock.Verify(x => x.NotificationRepository.UpdateAsync(It.IsAny<Notification>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.NotificationDetailRepository.UpdateAsync(It.IsAny<NotificationDetail>()), Times.Never);
        }

        [Fact]
        public async Task UpdateIsReadNoti_WhenExceptionOccurs_ThrowsInternalServerErrorException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateNotiRequest
            {
                AccountId = userId
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _unitOfWorkMock.Setup(x => x.NotificationDetailRepository.Get(
                    It.IsAny<System.Linq.Expressions.Expression<Func<NotificationDetail, bool>>>(),
                    It.IsAny<Func<IQueryable<NotificationDetail>, IOrderedQueryable<NotificationDetail>>>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Domain.CustomException.CustomException.InternalServerErrorException>(
                () => _notificationService.UpdateIsReadNoti(request)
            );

            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllNotiOfCus_Success_ReturnsNotificationDetailResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var account = new Domain.Entities.Account
            {
                AccountId = userId,
                Fullname = "testuser",
                Email = "test@example.com"
            };

            var notifications = new List<Notification>
            {
                new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    Title = "Test Notification 1",
                    Message = "Test Message 1",
                    Bar = new Domain.Entities.Bar { BarId = Guid.NewGuid() },
                    NotificationDetails = new List<NotificationDetail>
                    {
                        new NotificationDetail
                        {
                            AccountId = userId,
                            IsRead = PrefixKeyConstant.TRUE,
                            Account = account
                        }
                    }
                },
                new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    Title = "Test Notification 2",
                    Message = "Test Message 2",
                    Bar = new Domain.Entities.Bar { BarId = Guid.NewGuid() },
                    NotificationDetails = new List<NotificationDetail>
                    {
                        new NotificationDetail
                        {
                            AccountId = userId,
                            IsRead = PrefixKeyConstant.FALSE,
                            Account = account
                        }
                    }
                }
            };

            var expectedResponse = new NotificationDetailResponse
            {
                AccountId = userId,
                Fullname = account.Fullname,
                NotificationResponses = notifications.Select(n => new NotificationResponse
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.NotificationDetails.First().IsRead
                }).ToList()
            };

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _unitOfWorkMock.Setup(x => x.NotificationRepository.GetAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>(),
                    It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .ReturnsAsync(notifications);

            _mapperMock.Setup(x => x.Map<NotificationDetailResponse>(It.IsAny<Account>()))
                .Returns(new NotificationDetailResponse
                {
                    AccountId = account.AccountId,
                    Fullname = account.Fullname
                });

            _mapperMock.Setup(x => x.Map<List<NotificationResponse>>(It.IsAny<List<Notification>>()))
                .Returns(expectedResponse.NotificationResponses);

            // Act
            var result = await _notificationService.GetAllNotiOfCus(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.AccountId);
            Assert.Equal(account.Fullname, result.Fullname);
            Assert.Equal(2, result.NotificationResponses.Count);
            Assert.Contains(result.NotificationResponses, n => n.IsRead == PrefixKeyConstant.TRUE);
            Assert.Contains(result.NotificationResponses, n => n.IsRead == PrefixKeyConstant.FALSE);
        }

        [Fact]
        public async Task GetAllNotiOfCus_UnauthorizedUser_ThrowsUnAuthorizedException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            // Act & Assert
            await Assert.ThrowsAsync<Domain.CustomException.CustomException.UnAuthorizedException>(
                () => _notificationService.GetAllNotiOfCus(differentUserId)
            );
        }

        [Fact]
        public async Task GetAllNotiOfCus_DataNotFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _unitOfWorkMock.Setup(x => x.NotificationRepository.GetAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>(),
                    It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .ReturnsAsync(new List<Notification>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _notificationService.GetAllNotiOfCus(userId)
            );
        }

        [Fact]
        public async Task GetAllNotiOfCus_WhenExceptionOccurs_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _authenticationMock.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(userId);

            _unitOfWorkMock.Setup(x => x.NotificationRepository.GetAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>(),
                    It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _notificationService.GetAllNotiOfCus(userId)
            );
        }

        [Fact]
        public async Task CreateNotificationAllCustomer_Success_ReturnsNotificationResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new NotificationRequest
            {
                Title = "Test Notification",
                Message = "Test Message"
            };

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                Title = request.Title,
                Message = request.Message,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var expectedResponse = new NotificationResponse
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Message = notification.Message
            };

            _mapperMock.Setup(x => x.Map<Notification>(request))
                .Returns(notification);

            _mapperMock.Setup(x => x.Map<NotificationResponse>(notification))
                .Returns(expectedResponse);

            _unitOfWorkMock.Setup(x => x.NotificationRepository.InsertAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            _notificationDetailServiceMock
                .Setup(x => x.CreateNotificationDetailJob(It.IsAny<NotificationDetailRequest>()));

            // Act
            var result = await _notificationService.CreateNotificationAllCustomer(accountId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.NotificationId, result.NotificationId);
            Assert.Equal(expectedResponse.Title, result.Title);
            Assert.Equal(expectedResponse.Message, result.Message);

            _unitOfWorkMock.Verify(x => x.NotificationRepository.InsertAsync(It.IsAny<Notification>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
            _notificationDetailServiceMock.Verify(x => x.CreateNotificationDetailJob(It.IsAny<NotificationDetailRequest>()), Times.Once);
        }

        [Fact]
        public async Task CreateNotificationAllCustomer_WhenExceptionOccurs_ThrowsInternalServerErrorException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new NotificationRequest
            {
                Title = "Test Notification",
                Message = "Test Message"
            };

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                Title = request.Title,
                Message = request.Message,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _mapperMock.Setup(x => x.Map<Notification>(request))
                .Returns(notification);

            _unitOfWorkMock.Setup(x => x.NotificationRepository.InsertAsync(It.IsAny<Notification>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Domain.CustomException.CustomException.InternalServerErrorException>(
                () => _notificationService.CreateNotificationAllCustomer(accountId, request)
            );

            // Verify
            _unitOfWorkMock.Verify(x => x.NotificationRepository.InsertAsync(It.IsAny<Notification>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
            _notificationDetailServiceMock.Verify(x => x.CreateNotificationDetailJob(It.IsAny<NotificationDetailRequest>()), Times.Never);
        }
    }
}
