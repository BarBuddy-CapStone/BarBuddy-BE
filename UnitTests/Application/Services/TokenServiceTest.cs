using Application.DTOs.Authen;
using Application.DTOs.Tokens;
using Application.Interfaces;
using Application.Service;
using AutoMapper;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;

namespace UnitTests.Application.Services
{
    public class TokenServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAuthentication> _mockAuthentication;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly TokenService _tokenService;

        public TokenServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAuthentication = new Mock<IAuthentication>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _tokenService = new TokenService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockAuthentication.Object,
                _mockHttpContextAccessor.Object
            );
        }

        [Fact]
        public async Task GenerteDefaultToken_ValidRefreshToken_ReturnsLoginResponse()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var accountId = Guid.NewGuid();
            var account = new Account { AccountId = accountId, Role = new Role() };
            var loginResponse = new LoginResponse();
            var newAccessToken = "new_access_token";
            
            // Tạo valid token
            var validToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1) // Token còn hạn
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            // Setup cho IsValidRefreshToken và GetValidRefreshToken
            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { validToken });

            // Setup cho GetValidRefreshToken
            var tokenResponse = new TokenResponse();
            _mockMapper.Setup(x => x.Map<TokenResponse>(It.IsAny<Token>()))
                .Returns(tokenResponse);

            _mockUnitOfWork.Setup(x => x.AccountRepository.Get(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Account> { account }.AsQueryable());

            _mockAuthentication.Setup(x => x.GenerteDefaultToken(account))
                .Returns(newAccessToken);

            _mockMapper.Setup(x => x.Map<LoginResponse>(account))
                .Returns(loginResponse);

            // Act
            var result = await _tokenService.GenerteDefaultToken(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loginResponse, result);
            Assert.Equal(newAccessToken, result.AccessToken);
            Assert.Equal(refreshToken, result.RefreshToken);
        }

        [Fact]
        public async Task GenerteDefaultToken_InvalidRefreshToken_ThrowsUnAuthorizedException()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";
            var accountId = Guid.NewGuid();

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tokenService.GenerteDefaultToken(refreshToken));
        }

        [Fact]
        public async Task GenerteDefaultToken_AccountNotFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var accountId = Guid.NewGuid();
            var validToken = new Token
            {
                Tokens = refreshToken,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1) // Token còn hạn
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            // Setup cho IsValidRefreshToken
            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { validToken });

            // Setup cho GetValidRefreshToken
            var tokenResponse = new TokenResponse();
            _mockMapper.Setup(x => x.Map<TokenResponse>(It.IsAny<Token>()))
                .Returns(tokenResponse);

            // Setup AccountRepository trả về empty để tạo ra DataNotFoundException
            _mockUnitOfWork.Setup(x => x.AccountRepository.Get(
                It.IsAny<Expression<Func<Account, bool>>>(),
                It.IsAny<Func<IQueryable<Account>, IOrderedQueryable<Account>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Account>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tokenService.GenerteDefaultToken(refreshToken));
        }

        [Fact]
        public async Task GetValidRefreshToken_ValidToken_ReturnsTokenResponse()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var accountId = Guid.NewGuid();
            var validToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            var expectedResponse = new TokenResponse();

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { validToken });

            _mockMapper.Setup(x => x.Map<TokenResponse>(validToken))
                .Returns(expectedResponse);

            // Act
            var result = await _tokenService.GetValidRefreshToken(accountId, refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task GetValidRefreshToken_TokenNotFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";
            var accountId = Guid.NewGuid();

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tokenService.GetValidRefreshToken(accountId, refreshToken));
        }

        [Fact]
        public async Task GetValidRefreshToken_ExpiredToken_ReturnsTokenResponse()
        {
            // Arrange
            var refreshToken = "expired_refresh_token";
            var accountId = Guid.NewGuid();
            var expiredToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(-1) // Token đã hết hạn
            };
            var expectedResponse = new TokenResponse();

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { expiredToken });

            _mockMapper.Setup(x => x.Map<TokenResponse>(expiredToken))
                .Returns(expectedResponse);

            // Act
            var result = await _tokenService.GetValidRefreshToken(accountId, refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task GetValidRefreshToken_RevokedToken_ReturnsTokenResponse()
        {
            // Arrange
            var refreshToken = "revoked_refresh_token";
            var accountId = Guid.NewGuid();
            var revokedToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = true, // Token đã bị revoke
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            var expectedResponse = new TokenResponse();

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { revokedToken });

            _mockMapper.Setup(x => x.Map<TokenResponse>(revokedToken))
                .Returns(expectedResponse);

            // Act
            var result = await _tokenService.GetValidRefreshToken(accountId, refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task IsValidRefreshToken_ValidToken_ReturnsTrue()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var accountId = Guid.NewGuid();
            var validToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1) // Token còn hạn
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { validToken });

            // Act
            var result = await _tokenService.IsValidRefreshToken(refreshToken);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsValidRefreshToken_TokenNotFound_ThrowsUnAuthorizedException()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";
            var accountId = Guid.NewGuid();

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tokenService.IsValidRefreshToken(refreshToken));
            Assert.Equal("Refresh token không tồn tại !", exception.Message);
        }

        [Fact]
        public async Task IsValidRefreshToken_ExpiredToken_ThrowsUnAuthorizedException()
        {
            // Arrange
            var refreshToken = "expired_refresh_token";
            var accountId = Guid.NewGuid();
            var expiredToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(-1) // Token đã hết hạn
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { expiredToken });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tokenService.IsValidRefreshToken(refreshToken));
            Assert.Equal("Refresh token đã hết hạn, vui lòng đăng nhập lại !", exception.Message);
        }

        [Fact]
        public async Task IsValidRefreshToken_RevokedToken_ThrowsUnAuthorizedException()
        {
            // Arrange
            var refreshToken = "revoked_refresh_token";
            var accountId = Guid.NewGuid();
            var revokedToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = true,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { revokedToken });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tokenService.IsValidRefreshToken(refreshToken));
            Assert.Equal("Refresh token đã hết hạn hoặc đã dùng, vui lòng đăng nhập lại !", exception.Message);
        }

        [Fact]
        public async Task IsValidRefreshToken_UsedToken_ThrowsUnAuthorizedException()
        {
            // Arrange
            var refreshToken = "used_refresh_token";
            var accountId = Guid.NewGuid();
            var usedToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = true,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { usedToken });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tokenService.IsValidRefreshToken(refreshToken));
            Assert.Equal("Refresh token đã hết hạn hoặc đã dùng, vui lòng đăng nhập lại !", exception.Message);
        }

        [Fact]
        public async Task RevokeRefreshToken_ValidToken_ReturnsTrue()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var accountId = Guid.NewGuid();
            var validToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { validToken });

            _mockUnitOfWork.Setup(x => x.TokenRepository.Get(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Token> { validToken }.AsQueryable());

            // Act
            var result = await _tokenService.RevokeRefreshToken(refreshToken);

            // Assert
            Assert.True(result);
            Assert.True(validToken.IsRevoked);
            _mockUnitOfWork.Verify(x => x.TokenRepository.UpdateRangeAsync(validToken), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task RevokeRefreshToken_TokenNotFound_ThrowsDataNotFoundException()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";
            var accountId = Guid.NewGuid();
            var validToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            // Setup cho IsValidRefreshToken
            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { validToken });

            // Setup cho RevokeRefreshToken - token không tồn tại
            _mockUnitOfWork.Setup(x => x.TokenRepository.Get(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Token>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tokenService.RevokeRefreshToken(refreshToken));
        }

        [Fact]
        public async Task RevokeRefreshToken_EmptyToken_ThrowsInternalServerError()
        {
            // Arrange
            string refreshToken = "";
            var accountId = Guid.NewGuid();

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            // Setup cho IsValidRefreshToken
            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tokenService.RevokeRefreshToken(refreshToken));
            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }

        [Fact]
        public async Task RevokeRefreshToken_DatabaseError_ThrowsInternalServerError()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var accountId = Guid.NewGuid();
            var validToken = new Token
            {
                Tokens = refreshToken,
                AccountId = accountId,
                IsRevoked = false,
                IsUsed = false,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TokenRepository.GetAsync(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Token> { validToken });

            _mockUnitOfWork.Setup(x => x.TokenRepository.Get(
                It.IsAny<Expression<Func<Token, bool>>>(),
                It.IsAny<Func<IQueryable<Token>, IOrderedQueryable<Token>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Token> { validToken }.AsQueryable());

            _mockUnitOfWork.Setup(x => x.SaveAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tokenService.RevokeRefreshToken(refreshToken));
            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }

        [Fact]
        public async Task SaveRefreshToken_ValidData_ReturnsTokenResponse()
        {
            // Arrange
            var token = "new_refresh_token";
            var accountId = Guid.NewGuid();
            var expectedResponse = new TokenResponse();

            Token savedToken = null;
            _mockUnitOfWork.Setup(x => x.TokenRepository.InsertAsync(It.IsAny<Token>()))
                .Callback<Token>(token => savedToken = token);

            _mockMapper.Setup(x => x.Map<TokenResponse>(It.IsAny<Token>()))
                .Returns(expectedResponse);

            // Act
            var result = await _tokenService.SaveRefreshToken(token, accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse, result);

            // Verify token properties
            Assert.NotNull(savedToken);
            Assert.Equal(token, savedToken.Tokens);
            Assert.Equal(accountId, savedToken.AccountId);
            Assert.False(savedToken.IsRevoked);
            Assert.False(savedToken.IsUsed);
            Assert.True(savedToken.Created <= DateTime.UtcNow);
            Assert.True(savedToken.Expires > DateTime.Now.AddDays(13)); // Kiểm tra expires khoảng 14 ngày

            // Verify method calls
            _mockUnitOfWork.Verify(x => x.TokenRepository.InsertAsync(It.IsAny<Token>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task SaveRefreshToken_DatabaseError_ThrowsInternalServerError()
        {
            // Arrange
            var token = "new_refresh_token";
            var accountId = Guid.NewGuid();

            _mockUnitOfWork.Setup(x => x.TokenRepository.InsertAsync(It.IsAny<Token>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tokenService.SaveRefreshToken(token, accountId));
            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }

        [Fact]
        public async Task SaveRefreshToken_NullToken_ThrowsInternalServerError()
        {
            // Arrange
            string token = null;
            var accountId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tokenService.SaveRefreshToken(token, accountId));
            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }

        [Fact]
        public async Task SaveRefreshToken_EmptyGuid_ThrowsInternalServerError()
        {
            // Arrange
            var token = "new_refresh_token";
            var accountId = Guid.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tokenService.SaveRefreshToken(token, accountId));
            Assert.Equal("Lỗi hệ thống !", exception.Message);
        }
    }
}
