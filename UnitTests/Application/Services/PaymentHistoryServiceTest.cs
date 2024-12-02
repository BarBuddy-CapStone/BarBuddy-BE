using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.PaymentHistory;
using Application.Interfaces;
using Application.Service;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace UnitTests.Application.Services
{
    public class PaymentHistoryServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAuthentication> _mockAuthentication;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly PaymentHistoryService _service;

        public PaymentHistoryServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAuthentication = new Mock<IAuthentication>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            
            _service = new PaymentHistoryService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockAuthentication.Object,
                _mockHttpContextAccessor.Object
            );
        }

        [Fact]
        public async Task Get_Success_ReturnsPaymentHistoryList()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var payments = new List<PaymentHistory>
            {
                new PaymentHistory 
                { 
                    Account = new Account { Fullname = "Test User", Phone = "123456789" },
                    Booking = new Booking { BarId = barId },
                    TransactionCode = "TEST001",
                    Status = 1
                }
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(payments);

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(payments);

            _mockUnitOfWork.Setup(x => x.BarRepository.GetByIdAsync(barId))
                .ReturnsAsync(new Bar { BarId = barId, BarName = "Test Bar" });

            // Act
            var result = await _service.Get(
                Status: 1,
                CustomerName: null,
                PhoneNumber: null,
                Email: null,
                BarId: barId,
                PaymentDate: null,
                PageIndex: 1,
                PageSize: 10
            );

            // Assert
            Assert.NotNull(result.response);
            Assert.Single(result.response);
            Assert.Equal(1, result.totalPage);
        }

        [Fact]
        public async Task Get_UnauthorizedBarAccess_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var accountBarId = Guid.NewGuid();
            var requestedBarId = Guid.NewGuid();

            var account = new Account { AccountId = accountId, BarId = accountBarId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(() => _service.Get(
                Status: 1,
                CustomerName: null,
                PhoneNumber: null,
                Email: null,
                BarId: requestedBarId,
                PaymentDate: null,
                PageIndex: 1,
                PageSize: 10
            ));
        }

        [Fact]
        public async Task Get_NoData_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<PaymentHistory>());

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync((List<PaymentHistory>)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() => _service.Get(
                Status: 1,
                CustomerName: null,
                PhoneNumber: null,
                Email: null,
                BarId: null,
                PaymentDate: null,
                PageIndex: 1,
                PageSize: 10
            ));
        }

        [Fact]
        public async Task Get_WithPagination_ReturnsCorrectTotalPages()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var payments = Enumerable.Range(0, 10).Select(i => new PaymentHistory 
            { 
                Account = new Account { Fullname = $"User {i}", Phone = "123456789" },
                Booking = new Booking { BarId = barId },
                TransactionCode = $"TEST{i:000}",
                Status = 1
            }).ToList();

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(payments);

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(payments.Take(10).ToList());

            _mockUnitOfWork.Setup(x => x.BarRepository.GetByIdAsync(barId))
                .ReturnsAsync(new Bar { BarId = barId, BarName = "Test Bar" });

            // Act
            var result = await _service.Get(
                Status: 1,
                CustomerName: null,
                PhoneNumber: null,
                Email: null,
                BarId: barId,
                PaymentDate: null,
                PageIndex: 1,
                PageSize: 5
            );

            // Assert
            Assert.Equal(3, result.totalPage);
            Assert.Equal(10, result.response.Count);
        }

        [Fact]
        public async Task GetByCustomerId_Success_ReturnsPaymentHistoryList()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var role = new Role { RoleId = roleId, RoleName = "CUSTOMER" };
            var account = new Account { 
                AccountId = customerId, 
                RoleId = roleId,
                Fullname = "Test Customer",
                Phone = "123456789"
            };
            
            var payments = new List<PaymentHistory>
            {
                new PaymentHistory 
                { 
                    AccountId = customerId,
                    Account = account,
                    Booking = new Booking { BarId = barId },
                    TransactionCode = "TEST001",
                    Status = 1,
                    PaymentDate = DateTime.Now,
                    TotalPrice = 100000,
                    PaymentFee = 1000,
                    ProviderName = "MOMO"
                }
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(customerId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(customerId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.RoleRepository.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role> { role });

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account> { account });

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(payments);

            _mockUnitOfWork.Setup(x => x.BarRepository.GetByIdAsync(barId))
                .ReturnsAsync(new Bar { BarId = barId, BarName = "Test Bar" });

            // Act
            var result = await _service.GetByCustomerId(
                customerId: customerId,
                Status: 1,
                PageIndex: 1,
                PageSize: 10
            );

            // Assert
            Assert.NotNull(result.response);
            Assert.Single(result.response);
            Assert.Equal(1, result.totalPage);
            Assert.Equal("Test Customer", result.response[0].CustomerName);
            Assert.Equal("123456789", result.response[0].PhoneNumber);
        }

        [Fact]
        public async Task GetByCustomerId_UnauthorizedAccess_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var requestedCustomerId = Guid.NewGuid();

            var account = new Account { AccountId = accountId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(() => 
                _service.GetByCustomerId(
                    customerId: requestedCustomerId,
                    Status: null,
                    PageIndex: 1,
                    PageSize: 10
                ));
        }

        [Fact]
        public async Task GetByCustomerId_CustomerNotFound_ThrowsException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var role = new Role { RoleId = roleId, RoleName = "CUSTOMER" };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(customerId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(customerId))
                .Returns(new Account { AccountId = customerId });

            _mockUnitOfWork.Setup(x => x.RoleRepository.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role> { role });

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync((List<Account>)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() => 
                _service.GetByCustomerId(
                    customerId: customerId,
                    Status: null,
                    PageIndex: 1,
                    PageSize: 10
                ));
        }

        [Fact]
        public async Task GetByCustomerId_WithPagination_ReturnsCorrectTotalPages()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var role = new Role { RoleId = roleId, RoleName = "CUSTOMER" };
            var account = new Account { 
                AccountId = customerId, 
                RoleId = roleId,
                Fullname = "Test Customer",
                Phone = "123456789"
            };
            
            var payments = Enumerable.Range(0, 10).Select(i => new PaymentHistory 
            { 
                AccountId = customerId,
                Account = account,
                Booking = new Booking { BarId = barId },
                TransactionCode = $"TEST{i:000}",
                Status = 1
            }).ToList();

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(customerId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(customerId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.RoleRepository.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role> { role });

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account> { account });

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(payments);

            _mockUnitOfWork.Setup(x => x.BarRepository.GetByIdAsync(barId))
                .ReturnsAsync(new Bar { BarId = barId, BarName = "Test Bar" });

            // Act
            var result = await _service.GetByCustomerId(
                customerId: customerId,
                Status: null,
                PageIndex: 1,
                PageSize: 5
            );

            // Assert
            Assert.Equal(3, result.totalPage);
            Assert.Equal(10, result.response.Count);
        }

        [Fact]
        public async Task GetByBarId_Success_ReturnsPaymentHistoryList()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var payments = new List<PaymentHistory>
            {
                new PaymentHistory 
                { 
                    Account = new Account { Fullname = "Test User", Phone = "123456789" },
                    Booking = new Booking { 
                        BarId = barId,
                        Bar = new Bar { BarId = barId, BarName = "Test Bar" }
                    },
                    TransactionCode = "TEST001",
                    Status = 1
                }
            };

            var mappedResponse = new List<PaymentHistoryResponse>
            {
                new PaymentHistoryResponse
                {
                    CustomerName = "Test User",
                    PhoneNumber = "123456789",
                    BarName = "Test Bar",
                    TransactionCode = "TEST001",
                    Status = 1
                }
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(payments);

            _mockMapper.Setup(x => x.Map<List<PaymentHistoryResponse>>(It.IsAny<IEnumerable<PaymentHistory>>()))
                .Returns(mappedResponse);

            // Act
            var result = await _service.GetByBarId(barId, 1, 10);

            // Assert
            Assert.NotNull(result.response);
            Assert.Single(result.response);
            Assert.Equal(1, result.totalPage);
            Assert.Equal("Test User", result.response[0].CustomerName);
            Assert.Equal("Test Bar", result.response[0].BarName);
        }

        [Fact]
        public async Task GetByBarId_UnauthorizedAccess_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var accountBarId = Guid.NewGuid();
            var requestedBarId = Guid.NewGuid();

            var account = new Account { AccountId = accountId, BarId = accountBarId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(() => 
                _service.GetByBarId(requestedBarId, 1, 10));
        }

        [Fact]
        public async Task GetByBarId_NoData_ReturnsEmptyList()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<PaymentHistory>());

            // Act
            var result = await _service.GetByBarId(barId, 1, 10);

            // Assert
            Assert.Empty(result.response);
            Assert.Equal(0, result.totalPage);
        }

        [Fact]
        public async Task GetByBarId_WithPagination_ReturnsCorrectTotalPages()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var payments = Enumerable.Range(0, 15).Select(i => new PaymentHistory 
            { 
                Account = new Account { Fullname = $"User {i}", Phone = "123456789" },
                Booking = new Booking { 
                    BarId = barId,
                    Bar = new Bar { BarId = barId, BarName = "Test Bar" }
                },
                TransactionCode = $"TEST{i:000}",
                Status = 1
            }).ToList();

            var mappedResponse = payments.Select(p => new PaymentHistoryResponse
            {
                CustomerName = p.Account.Fullname,
                PhoneNumber = p.Account.Phone,
                BarName = p.Booking.Bar.BarName,
                TransactionCode = p.TransactionCode,
                Status = p.Status
            }).ToList();

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.PaymentHistoryRepository.GetAsync(
                It.IsAny<Expression<Func<PaymentHistory, bool>>>(),
                It.IsAny<Func<IQueryable<PaymentHistory>, IOrderedQueryable<PaymentHistory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(payments);

            _mockMapper.Setup(x => x.Map<List<PaymentHistoryResponse>>(It.IsAny<IEnumerable<PaymentHistory>>()))
                .Returns(mappedResponse.Take(5).ToList());

            // Act
            var result = await _service.GetByBarId(barId, 1, 5);

            // Assert
            Assert.Equal(4, result.totalPage);
            Assert.Equal(5, result.response.Count);
        }
    }
}
