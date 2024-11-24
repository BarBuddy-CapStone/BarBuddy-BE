using Application.DTOs.Authen;
using Application.DTOs.Tokens;
using Application.Interfaces;
using Application.IService;
using Application.Service;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
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
    public class AuthenServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IAuthentication> _authenticationMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<IOtpSender> _otpSenderMock;
        private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IFcmService> _fcmServiceMock;
        private readonly AuthenService _authenService;
        private readonly Mock<IGenericRepository<Account>> _accountRepoMock;
        private readonly Mock<IGenericRepository<Role>> _roleRepoMock;

        public AuthenServiceTests() 
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _authenticationMock = new Mock<IAuthentication>();
            _cacheMock = new Mock<IMemoryCache>();
            _otpSenderMock = new Mock<IOtpSender>();
            _googleAuthServiceMock = new Mock<IGoogleAuthService>();
            _emailSenderMock = new Mock<IEmailSender>();
            _tokenServiceMock = new Mock<ITokenService>();
            _fcmServiceMock = new Mock<IFcmService>();

            _accountRepoMock = new Mock<IGenericRepository<Account>>();
            _roleRepoMock = new Mock<IGenericRepository<Role>>();

            // Create a mock configuration if needed
            var configurationMock = new Mock<IConfiguration>();

            _authenService = new AuthenService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _authenticationMock.Object,
                _cacheMock.Object,
                _otpSenderMock.Object,
                _googleAuthServiceMock.Object,
                _emailSenderMock.Object,
                _tokenServiceMock.Object,
                configurationMock.Object,  // Pass mock configuration
                _fcmServiceMock.Object
            );

            _unitOfWorkMock.Setup(x => x.AccountRepository).Returns(_accountRepoMock.Object);
            _unitOfWorkMock.Setup(x => x.RoleRepository).Returns(_roleRepoMock.Object);
        }

        [Fact]
        public async Task Login_WhenValidCredentials_ShouldReturnLoginResponse()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@test.com", Password = "password" };
            var hashedPassword = "hashedPassword";
            var account = new Account
            {
                AccountId = Guid.NewGuid(),
                Email = request.Email,
                Status = (int)PrefixValueEnum.Active
            };
            var token = "token";
            var loginResponse = new LoginResponse { AccessToken = token };
            var tokenResponse = new TokenResponse
            {
                TokenId = Guid.NewGuid(),
                AccountId = account.AccountId,
                AccessToken = token,
                Tokens = token,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false
            };

            _authenticationMock.Setup(x => x.HashedPassword(request.Password))
                .ReturnsAsync(hashedPassword);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.Is<string>(s => s == "Role,Bar"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account> { account });

            _authenticationMock.Setup(x => x.GenerteDefaultToken(It.IsAny<Account>()))
                .Returns(token);

            _mapperMock.Setup(x => x.Map<LoginResponse>(It.IsAny<Account>()))
                .Returns(loginResponse);

            // Setup cho TokenService với TokenResponse
            _tokenServiceMock.Setup(x => x.SaveRefreshToken(It.IsAny<string>(), account.AccountId))
                .ReturnsAsync(tokenResponse);

            // Act
            var result = await _authenService.Login(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(token, result.AccessToken);
            _tokenServiceMock.Verify(x => x.SaveRefreshToken(It.IsAny<string>(), account.AccountId), Times.Once);
        }
        [Fact]
        public async Task Login_WhenInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@test.com", Password = "wrong_password" };
            var hashedPassword = "hashedPassword";

            _authenticationMock.Setup(x => x.HashedPassword(request.Password))
                .ReturnsAsync(hashedPassword);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.Is<string>(s => s == "Role,Bar"),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                async () => await _authenService.Login(request));
        }

        [Fact]
        public async Task RegisterWithOtp_WhenNewUser_ShouldCreateAccountAndSendOtp()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@test.com",
                Password = "password",
                ConfirmPassword = "password"
            };
            var role = new Role { RoleId = Guid.NewGuid(), RoleName = "CUSTOMER" };
            var hashedPassword = "hashedPassword";
            var account = new Account
            {
                Email = request.Email,
                Password = hashedPassword,
                RoleId = role.RoleId,
                Status = (int)PrefixValueEnum.Pending
            };

            _accountRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Account>());

            _roleRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Role> { role });

            _authenticationMock.Setup(x => x.HashedPassword(request.Password))
                .ReturnsAsync(hashedPassword);

            _mapperMock.Setup(x => x.Map<Account>(request))
                .Returns(account);

            // Act
            var result = await _authenService.RegisterWithOtp(request);

            // Assert
            Assert.True(result);
            _unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            _accountRepoMock.Verify(x => x.InsertAsync(It.IsAny<Account>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.CommitTransaction(), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
            _otpSenderMock.Verify(x => x.SendOtpAsync(request.Email), Times.Once);
        }

        [Fact]
        public async Task RegisterWithOtp_WhenPasswordsDoNotMatch_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@test.com",
                Password = "password1",
                ConfirmPassword = "password2"
            };
            var role = new Role { RoleId = Guid.NewGuid(), RoleName = "CUSTOMER" };

            _roleRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Role> { role });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                async () => await _authenService.RegisterWithOtp(request));
        }

        [Fact]
        public async Task RegisterWithOtp_WhenEmailExists_ShouldThrowException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@test.com",
                Password = "password",
                ConfirmPassword = "password"
            };
            var existingAccount = new Account
            {
                Email = request.Email,
                Status = (int)PrefixValueEnum.Active
            };
            var role = new Role { RoleId = Guid.NewGuid(), RoleName = "CUSTOMER" };

            _accountRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Account> { existingAccount });

            _roleRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Role> { role });

            // Act & Assert
            await Assert.ThrowsAsync<DataExistException>(
                async () => await _authenService.RegisterWithOtp(request));
        }

        [Fact]
        public async Task GoogleLogin_WhenExistingUser_ShouldUpdateAndReturnLoginResponse()
        {
            // Arrange
            var idToken = "google-token";
            var googleUser = new GoogleUserRequest
            {
                Email = "test@gmail.com",
                Name = "Test User",
                PictureUrl = "picture-url"
            };
            var existingAccount = new Account
            {
                AccountId = Guid.NewGuid(),
                Email = googleUser.Email,
                Status = (int)PrefixValueEnum.Active
            };
            var loginResponse = new LoginResponse { AccessToken = "jwt-token" };

            // Setup GoogleAuthService authentication
            _googleAuthServiceMock.Setup(x => x.AuthenticateGoogleUserAsync(idToken))
                .ReturnsAsync(googleUser);

            _unitOfWorkMock.Setup(x => x.AccountRepository.GetAsync(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Account> { existingAccount });

            _googleAuthServiceMock.Setup(x => x.UploadProfilePictureAsync(
                googleUser.PictureUrl, googleUser.Email))
                .ReturnsAsync("new-picture-url");

            _mapperMock.Setup(x => x.Map<LoginResponse>(It.IsAny<Account>()))
                .Returns(loginResponse);

            _authenticationMock.Setup(x => x.GenerteDefaultToken(It.IsAny<Account>()))
                .Returns("jwt-token");

            // Act
            var result = await _authenService.GoogleLogin(idToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loginResponse.AccessToken, result.AccessToken);
            _unitOfWorkMock.Verify(x => x.AccountRepository.UpdateAsync(It.IsAny<Account>()), Times.Once);
        }

        [Fact]
        public async Task GoogleLogin_WhenGoogleAuthFails_ShouldThrowException()
        {
            // Arrange
            var idToken = "invalid_token";

            _googleAuthServiceMock.Setup(x => x.AuthenticateGoogleUserAsync(idToken))
                .ThrowsAsync(new Exception("Invalid token"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                async () => await _authenService.GoogleLogin(idToken));
        }

        [Fact]
        public async Task GoogleLogin_WhenRoleNotFound_ShouldThrowException()
        {
            // Arrange
            var idToken = "google-token";
            var googleUser = new GoogleUserRequest
            {
                Email = "test@gmail.com",
                Name = "Test User",
                PictureUrl = "picture-url"
            };

            _googleAuthServiceMock.Setup(x => x.AuthenticateGoogleUserAsync(idToken))
                .ReturnsAsync(googleUser);

            _roleRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Func<IQueryable<Role>, IOrderedQueryable<Role>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Role>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                async () => await _authenService.GoogleLogin(idToken));
        }
    }
}
