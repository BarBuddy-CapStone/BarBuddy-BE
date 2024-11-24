using Application.DTOs.Account;
using Application.Interfaces;
using Application.Service;
using AutoMapper;
using Domain.Entities;
using Domain.IRepository;
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
    public class AccountServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<Account>> _accountRepoMock;
        private readonly Mock<IGenericRepository<Bar>> _barRepoMock;
        private readonly Mock<IGenericRepository<Role>> _roleRepoMock;
        private readonly Mock<IFirebase> _firebaseMock;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _accountRepoMock = new Mock<IGenericRepository<Account>>();
            _barRepoMock = new Mock<IGenericRepository<Bar>>();
            _roleRepoMock = new Mock<IGenericRepository<Role>>();
            _firebaseMock = new Mock<IFirebase>();

            _unitOfWorkMock.Setup(x => x.AccountRepository).Returns(_accountRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.BarRepository).Returns(_barRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.RoleRepository).Returns(_roleRepoMock.Object);

            _accountService = new AccountService(
                _mapperMock.Object,
                _unitOfWorkMock.Object,
                _firebaseMock.Object
            );
        }

        [Fact]
        public async Task CreateCustomerAccount_WhenEmailExists_ShouldThrowDataExistException()
        {
            var request = new CustomerAccountRequest { Email = "test@test.com" };
            var existingAccount = new Account { Email = "test@test.com" };

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account> { existingAccount });

            _unitOfWorkMock.Setup(x => x.RoleRepository.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role> { new Role { RoleId = Guid.NewGuid(), RoleName = "CUSTOMER" } });

            await Assert.ThrowsAsync<DataExistException>(() =>
                _accountService.CreateCustomerAccount(request));
        }

        [Fact]
        public async Task CreateCustomerAccount_WhenValidRequest_ShouldCreateAccount()
        {
            var request = new CustomerAccountRequest { Email = "test@test.com" };
            var customerRole = new Role { RoleId = Guid.NewGuid(), RoleName = "CUSTOMER" };

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account>());

            _roleRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role> { customerRole });

            var mappedAccount = new Account();
            _mapperMock.Setup(x => x.Map<Account>(request)).Returns(mappedAccount);

            var expectedResponse = new CustomerAccountResponse();
            _mapperMock.Setup(x => x.Map<CustomerAccountResponse>(mappedAccount))
                .Returns(expectedResponse);

            var result = await _accountService.CreateCustomerAccount(request);

            Assert.NotNull(result);
            _accountRepoMock.Verify(x => x.Insert(It.IsAny<Account>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        [Fact]
        public async Task CreateCustomerAccount_WhenExceptionOccurs_ShouldRollbackAndThrow()
        {
            var request = new CustomerAccountRequest
            {
                Email = "test@example.com",
                Fullname = "Test User",
                Phone = "0123456789",
                Dob = DateTime.Now.AddYears(-20),
                Image = "default-avatar.jpg"
            };

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account>());

            _accountRepoMock.Setup(x => x.Insert(It.IsAny<Account>()))
                .Throws(new NullReferenceException("Test exception"));

            await Assert.ThrowsAsync<NullReferenceException>(() =>
                _accountService.CreateCustomerAccount(request));

        }

        [Fact]
        public async Task UpdateStaffAccount_WhenAccountNotFound_ShouldThrowDataNotFoundException()
        {
            
            var accountId = Guid.NewGuid();
            var request = new StaffAccountRequest { Email = "staff@test.com", BarId = Guid.NewGuid() };

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account>());

            
            await Assert.ThrowsAsync<DataNotFoundException>(() =>
                _accountService.UpdateStaffAccount(accountId, request));
        }

        [Fact]
        public async Task UpdateStaffAccount_WhenEmailDoesNotMatch_ShouldThrowDataNotFoundException()
        {
            
            var accountId = Guid.NewGuid();
            var request = new StaffAccountRequest { Email = "new@test.com", BarId = Guid.NewGuid() };
            var existingAccount = new Account { Email = "old@test.com" };

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account> { existingAccount });

            
            await Assert.ThrowsAsync<DataNotFoundException>(() =>
                _accountService.UpdateStaffAccount(accountId, request));
        }

        [Fact]
        public async Task UpdateStaffAccount_WhenBarNotFound_ShouldThrowDataNotFoundException()
        {
            
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var request = new StaffAccountRequest { Email = "staff@test.com", BarId = barId };
            var existingAccount = new Account { Email = "staff@test.com" };

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account> { existingAccount });

            _barRepoMock.Setup(x => x.GetByIdAsync(barId))
                .ReturnsAsync((Bar)null);

            
            await Assert.ThrowsAsync<DataNotFoundException>(() =>
                _accountService.UpdateStaffAccount(accountId, request));
        }

        [Fact]
        public async Task UpdateStaffAccount_WhenValidRequest_ShouldUpdateAndReturnResponse()
        {
            
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var request = new StaffAccountRequest { Email = "staff@test.com", BarId = barId };
            var existingAccount = new Account { Email = "staff@test.com" };
            var existingBar = new Bar { BarId = barId };
            var updatedAccount = new Account();
            var expectedResponse = new StaffAccountResponse();

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account> { existingAccount });

            _barRepoMock.Setup(x => x.GetByIdAsync(barId))
                .ReturnsAsync(existingBar);

            _mapperMock.Setup(x => x.Map(request, existingAccount))
                .Returns(updatedAccount);

            _mapperMock.Setup(x => x.Map<StaffAccountResponse>(updatedAccount))
                .Returns(expectedResponse);

            
            var result = await _accountService.UpdateStaffAccount(accountId, request);

            
            Assert.NotNull(result);
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _accountRepoMock.Verify(x => x.Update(updatedAccount), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task BlockOrUnblockAccount_WhenAccountNotFound_ShouldThrowDataNotFoundException()
        {
            
            var accountId = "test-id";
            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns((Account)null);

            
            await Assert.ThrowsAsync<DataNotFoundException>(() =>
                _accountService.BlockOrUnblockAccount(accountId, true));
        }

        [Theory]
        [InlineData(true, 0)]  // Block -> status = 0
        [InlineData(false, 1)] // Unblock -> status = 1
        public async Task BlockOrUnblockAccount_WhenValidRequest_ShouldUpdateStatus(bool isBlock, int expectedStatus)
        {
            
            var accountId = "552a1ce5-d02c-47e0-ae44-d6c2e87e2218";
            var account = new Account { AccountId = Guid.Parse(accountId), Status = 1 };

            _accountRepoMock.Setup(x => x.GetByID(accountId))
                .Returns(account);

            
            var result = await _accountService.BlockOrUnblockAccount(accountId, isBlock);

            
            Assert.True(result);
            Assert.Equal(expectedStatus, account.Status);
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _accountRepoMock.Verify(x => x.UpdateAsync(It.Is<Account>(a => a.Status == expectedStatus)), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.Save(), Times.Once);
        }

        [Fact]
        public async Task GetPaginationStaffAccount_WhenRoleNotFound_ShouldThrowDataNotFoundException()
        {
            
            _unitOfWorkMock.Setup(x => x.RoleRepository.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role>());

            
            await Assert.ThrowsAsync<DataNotFoundException>(() =>
                _accountService.GetPaginationStaffAccount(10, 1));
        }

        [Fact]
        public async Task GetPaginationStaffAccount_WhenNoStaffAccounts_ShouldThrowDataNotFoundException()
        {
            
            var staffRole = new Role { RoleId = Guid.NewGuid(), RoleName = "STAFF" };

            _unitOfWorkMock.Setup(x => x.RoleRepository.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role> { staffRole });

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account>());

            
            await Assert.ThrowsAsync<DataNotFoundException>(() =>
                _accountService.GetPaginationStaffAccount(10, 1));
        }

        [Fact]
        public async Task GetPaginationStaffAccount_WhenValidRequest_ShouldReturnPaginatedList()
        {
            var staffRole = new Role { RoleId = Guid.NewGuid(), RoleName = "STAFF" };
            var staffAccounts = new List<Account> { new Account(), new Account() };
            var expectedResponses = new List<StaffAccountResponse> { new StaffAccountResponse(), new StaffAccountResponse() };

            _unitOfWorkMock.Setup(x => x.RoleRepository.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role> { staffRole });

            _accountRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(staffAccounts);

            _mapperMock.Setup(x => x.Map<IEnumerable<StaffAccountResponse>>(staffAccounts))
                .Returns(expectedResponses);

            var result = await _accountService.GetPaginationStaffAccount(10, 1);

            Assert.NotNull(result);
            Assert.Equal(expectedResponses, result.items);
            Assert.Equal(expectedResponses.Count, result.total);
        }

        [Fact]
        public async Task GetCustomerInfoById_WhenAccountNotFound_ShouldThrowDataNotFoundException()
        {
            var accountId = Guid.NewGuid();

            _accountRepoMock.Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            var exception = await Assert.ThrowsAsync<DataNotFoundException>(() =>
                _accountService.GetCustomerInfoById(accountId));

            Assert.Equal("Khách hàng không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task GetCustomerInfoById_WhenAccountNotHaveRole_ShouldThrowForbbidenException()
        {
            var accountId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var account = new Account
            {
                AccountId = accountId,
                RoleId = roleId
            };
            var role = new Role
            {
                RoleId = roleId,
                RoleName = "STAFF"
            };

            _accountRepoMock.Setup(x => x.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            _unitOfWorkMock.Setup(x => x.RoleRepository.GetByIdAsync(roleId))
                .ReturnsAsync(role);

            var exception = await Assert.ThrowsAsync<ForbbidenException>(() =>
                _accountService.GetCustomerInfoById(accountId));

            Assert.Equal("Bạn không thể truy cập", exception.Message);
        }
    }
}
