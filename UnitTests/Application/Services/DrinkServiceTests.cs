using Application.DTOs.Drink;
using Application.DTOs.DrinkCategory;
using Application.DTOs.Response.EmotionCategory;
using Application.Interfaces;
using Application.Service;
using AutoMapper;
using Domain.Common;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
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
    public class DrinkServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IFirebase> _firebaseMock;
        private readonly Mock<IHttpContextAccessor> _contextAccessorMock;
        private readonly Mock<IAuthentication> _authenticationMock;
        private readonly DrinkService _drinkService;
        public DrinkServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _firebaseMock = new Mock<IFirebase>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _authenticationMock = new Mock<IAuthentication>();
            _drinkService = new DrinkService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _firebaseMock.Object,
                _authenticationMock.Object,
                _contextAccessorMock.Object
            );
        }

        [Fact]
        public async Task CreateDrink_ValidRequest_ShouldReturnDrinkResponse()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var request = new DrinkRequest
            {
                BarId = barId,
                DrinkCategoryId = Guid.NewGuid(),
                DrinkBaseEmo = new List<Guid> { Guid.NewGuid() },
                Images = new List<IFormFile> { Mock.Of<IFormFile>() }
            };
            var account = new Account
            {
                AccountId = accountId,
                BarId = barId,
            };
            var drink = new Drink { DrinkId = Guid.NewGuid() };
            var drinkCategory = new DrinkCategory();
            var emotionCategory = new EmotionalDrinkCategory();
            
            _unitOfWorkMock.Setup(u => u.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
               .Returns(new List<Bar> { new Bar { BarId = request.BarId, Status = PrefixKeyConstant.TRUE } }.AsQueryable());
            
            _unitOfWorkMock.Setup(u => u.DrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
               .Returns(new List<DrinkCategory> { drinkCategory }.AsQueryable());

            _unitOfWorkMock.Setup(u => u.EmotionalDrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
               .Returns(new List<EmotionalDrinkCategory> { emotionCategory }.AsQueryable());

            _unitOfWorkMock.Setup(u => u.DrinkEmotionalCategoryRepository.InsertAsync(It.IsAny<DrinkEmotionalCategory>()));

            _authenticationMock.Setup(a => a.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);
            
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByID(accountId))
                .Returns(account);

            _unitOfWorkMock.Setup(u => u.DrinkRepository.InsertAsync(drink));

            _mapperMock.Setup(m => m.Map<Drink>(It.IsAny<DrinkRequest>())).Returns(drink);
            _mapperMock.Setup(m => m.Map<DrinkResponse>(It.IsAny<Drink>())).Returns(new DrinkResponse());
            // Act
            var result = await _drinkService.CreateDrink(request);
            // Assert
            Assert.NotNull(result);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateDrink_InvalidImages_ShouldThrowInvalidDataException()
        {
            // Arrange
            var request = new DrinkRequest
            {
                Images = null
            };
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(() => _drinkService.CreateDrink(request));
        }

        [Fact]
        public async Task CreateDrink_BarNotFound_ShouldThrowDataNotFoundException()
        {
            var request = new DrinkRequest
            {
                BarId = Guid.NewGuid(),
                Images = new List<IFormFile> { Mock.Of<IFormFile>() }
            };
            _unitOfWorkMock.Setup(u => u.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
               .Returns(new List<Bar>().AsQueryable());
            _authenticationMock.Setup(a => a.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
               .Returns(Guid.NewGuid());
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByID(It.IsAny<Guid>()))
               .Returns(new Account { BarId = request.BarId });
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() => _drinkService.CreateDrink(request));
        }

        [Fact]
        public async Task CreateDrink_UnauthorizedAccess_ShouldThrowUnAuthorizedException()
        {
            // Arrange
            var request = new DrinkRequest
            {
                BarId = Guid.NewGuid(),
                Images = new List<IFormFile> { Mock.Of<IFormFile>() }
            };
            var account = new Account { BarId = Guid.NewGuid() };
            _unitOfWorkMock.Setup(u => u.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
               .Returns(new List<Bar> { new Bar { BarId = request.BarId, Status = PrefixKeyConstant.TRUE } }.AsQueryable());
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByID(It.IsAny<Guid>()))
               .Returns(account);
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(() => _drinkService.CreateDrink(request));
        }

        [Fact]
        public async Task GetAllDrink_ValidRequest_ShouldReturnDrinkResponses()
        {
            // Arrange
            var drinks = new List<Drink>
           {
               new Drink { DrinkId = Guid.NewGuid(), DrinkName = "Drink1" },
               new Drink { DrinkId = Guid.NewGuid(), DrinkName = "Drink2" }
           };
            var queryDrinkRequest = new QueryDrinkRequest();
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(drinks);
            _mapperMock.Setup(m => m.Map<IEnumerable<DrinkResponse>>(It.IsAny<IEnumerable<Drink>>()))
               .Returns((IEnumerable<Drink> source) => source.Select(d => new DrinkResponse { DrinkId = d.DrinkId, DrinkName = d.DrinkName }));
            // Act
            var result = await _drinkService.GetAllDrink(queryDrinkRequest);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllDrink_NoData_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var queryDrinkRequest = new QueryDrinkRequest();
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Drink>());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() => _drinkService.GetAllDrink(queryDrinkRequest));
        }

        [Fact]
        public async Task GetAllDrinkCustomer_ValidRequest_ShouldReturnDrinkResponses()
        {
            // Arrange
            var drinks = new List<Drink>
           {
               new Drink { DrinkId = Guid.NewGuid(), DrinkName = "Drink1", Status = PrefixKeyConstant.TRUE },
               new Drink { DrinkId = Guid.NewGuid(), DrinkName = "Drink2", Status = PrefixKeyConstant.TRUE }
           };
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(drinks);
            _mapperMock.Setup(m => m.Map<IEnumerable<DrinkResponse>>(It.IsAny<IEnumerable<Drink>>()))
               .Returns((IEnumerable<Drink> source) => source.Select(d => new DrinkResponse { DrinkId = d.DrinkId, DrinkName = d.DrinkName }));
            // Act
            var result = await _drinkService.GetAllDrinkCustomer();
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllDrinkCustomer_NoData_ShouldReturnEmptyList()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Drink>());
            _mapperMock.Setup(m => m.Map<IEnumerable<DrinkResponse>>(It.IsAny<IEnumerable<Drink>>()))
               .Returns(new List<DrinkResponse>());
            // Act
            var result = await _drinkService.GetAllDrinkCustomer();
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDrinkCustomerOfBar_ValidRequest_ShouldReturnDrinkResponses()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var drinks = new List<Drink>
           {
               new Drink { DrinkId = Guid.NewGuid(), DrinkName = "Drink1", Status = PrefixKeyConstant.TRUE, Bar = new Bar { BarId = barId } },
               new Drink { DrinkId = Guid.NewGuid(), DrinkName = "Drink2", Status = PrefixKeyConstant.TRUE, Bar = new Bar { BarId = barId } }
           };
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(drinks);
            _mapperMock.Setup(m => m.Map<IEnumerable<DrinkResponse>>(It.IsAny<IEnumerable<Drink>>()))
               .Returns((IEnumerable<Drink> source) => source.Select(d => new DrinkResponse { DrinkId = d.DrinkId, DrinkName = d.DrinkName }));
            // Act
            var result = await _drinkService.GetDrinkCustomerOfBar(barId);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetDrinkCustomerOfBar_NoData_ShouldReturnEmptyList()
        {
            // Arrange
            var barId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(new List<Drink>());
            _mapperMock.Setup(m => m.Map<IEnumerable<DrinkResponse>>(It.IsAny<IEnumerable<Drink>>()))
               .Returns(new List<DrinkResponse>());
            // Act
            var result = await _drinkService.GetDrinkCustomerOfBar(barId);
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllDrinkBasedCateId_ValidRequest_ShouldReturnDrinkResponses()
        {
            // Arrange
            ObjectQueryCustom queryCustom = new ObjectQueryCustom();
            var cateId = Guid.NewGuid();
            var drinks = new List<Drink>
            {
                new Drink 
                { 
                    DrinkId = Guid.NewGuid(), 
                    DrinkName = "Drink1", 
                    DrinkCategory = new DrinkCategory { DrinksCategoryId = cateId } 
                },
                new Drink 
                { 
                    DrinkId = Guid.NewGuid(), 
                    DrinkName = "Drink2", 
                    DrinkCategory = new DrinkCategory { DrinksCategoryId = cateId } 
                }
            };

            // Tạo expected response để mock mapper
            var expectedDrinkResponses = drinks.Select(d => new DrinkResponse
            {
                DrinkId = d.DrinkId,
                DrinkName = d.DrinkName,
                DrinkCategoryResponse = new DrinkCategoryResponse 
                { 
                    DrinksCategoryId = d.DrinkCategory.DrinksCategoryId.ToString() 
                }
            }).ToList();

            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(drinks);

            // Mock mapper để trả về expected response
            _mapperMock.Setup(m => m.Map<List<DrinkResponse>>(drinks))
                .Returns(expectedDrinkResponses);

            // Act
            var result = await _drinkService.GetAllDrinkBasedCateId(cateId, queryCustom);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.DrinkResponses.Count());
            Assert.Equal(expectedDrinkResponses, result.DrinkResponses);
        }

        [Fact]
        public async Task GetAllDrinkBasedCateId_NoData_ShouldThrowDataNotFoundException()
        {
            // Arrange
            ObjectQueryCustom queryCustom = new ObjectQueryCustom();
            var cateId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(new List<Drink>());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() => _drinkService.GetAllDrinkBasedCateId(cateId, queryCustom));
        }

        [Fact]
        public async Task GetDrink_ValidRequest_ShouldReturnDrinkResponse()
        {
            // Arrange
            var drinkId = Guid.NewGuid();
            var drinks = new List<Drink>
           {
               new Drink 
               { 
                   DrinkId = drinkId, 
                   DrinkName = "Drink1",
                   DrinkEmotionalCategories = new List<DrinkEmotionalCategory>
                   {
                        new DrinkEmotionalCategory
                        {
                            EmotionalDrinkCategoryId = Guid.NewGuid(),
                            EmotionalDrinkCategory = new EmotionalDrinkCategory
                            {
                                CategoryName = "Test"
                            }
                        }
                    }
               },
               
           };
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(drinks);
            _mapperMock.Setup(m => m.Map<DrinkResponse>(It.IsAny<Drink>()))
               .Returns((Drink source) => new DrinkResponse { DrinkId = source.DrinkId, DrinkName = source.DrinkName });
            // Act
            var result = await _drinkService.GetDrink(drinkId);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(drinkId, result.DrinkId);
        }

        [Fact]
        public async Task GetDrink_NoData_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var drinkId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(new List<Drink>());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() => _drinkService.GetDrink(drinkId));
        }

        [Fact]
        public async Task UpdateDrink_ValidRequest_ShouldReturnDrinkResponse()
        {
            // Arrange
            var drinkId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var drinkCategoryId = Guid.NewGuid();
            var emotionId = Guid.NewGuid();
            var request = new DrinkRequest
            {
                BarId = barId,
                DrinkCategoryId = drinkCategoryId,
                OldImages = new List<string> { "image1.jpg" },
                Images = new List<IFormFile> { Mock.Of<IFormFile>() },
                DrinkBaseEmo = new List<Guid> { emotionId }
            };
            var account = new Account
            {
                AccountId = accountId,
                BarId = barId
            };
            var bar = new Bar
            {
                BarId = barId,
                Status = PrefixKeyConstant.TRUE
            };
            var drink = new Drink
            {
                DrinkId = drinkId,
                BarId = barId,
                Image = "image1.jpg"
            };
            var drinkCategory = new DrinkCategory
            {
                DrinksCategoryId = drinkCategoryId,
                IsDeleted = PrefixKeyConstant.FALSE
            };
            var emotionalCategory = new EmotionalDrinkCategory
            {
                EmotionalDrinksCategoryId = emotionId,
                IsDeleted = PrefixKeyConstant.FALSE
            };
            var drinkEmotionalCategory = new DrinkEmotionalCategory
            {
                DrinkId = drinkId,
                EmotionalDrinkCategoryId = emotionId
            };
            // Mock authentication
            _authenticationMock.Setup(a => a.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);
            // Mock account repository
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByID(accountId))
                .Returns(account);
            // Mock bar repository
            _unitOfWorkMock.Setup(u => u.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());
            // Mock drink repository
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Drink> { drink });
            // Mock drink category repository
            _unitOfWorkMock.Setup(u => u.DrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<DrinkCategory> { drinkCategory }.AsQueryable());
            // Mock emotional drink category repository
            _unitOfWorkMock.Setup(u => u.EmotionalDrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EmotionalDrinkCategory> { emotionalCategory }.AsQueryable());
            // Mock drink emotional category repository
            _unitOfWorkMock.Setup(u => u.DrinkEmotionalCategoryRepository.GetAsync(
                It.IsAny<Expression<Func<DrinkEmotionalCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkEmotionalCategory>, IOrderedQueryable<DrinkEmotionalCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<DrinkEmotionalCategory> { drinkEmotionalCategory });
            // Mock firebase service
            _firebaseMock.Setup(f => f.UploadImageAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("new-image-url.jpg");
            // Mock mapper
            _mapperMock.Setup(m => m.Map(It.IsAny<DrinkRequest>(), It.IsAny<Drink>()))
                .Returns(drink);
            _mapperMock.Setup(m => m.Map<DrinkResponse>(It.IsAny<Drink>()))
                .Returns(new DrinkResponse
                {
                    DrinkId = drinkId,
                    DrinkName = "Test Drink",
                    Images = "image1.jpg,new-image-url.jpg"
                });
            // Act
            var result = await _drinkService.UpdateDrink(drinkId, request);
            // Assert
            Assert.NotNull(result);
            _unitOfWorkMock.Verify(u => u.SaveAsync(), Times.AtLeastOnce);
            _unitOfWorkMock.Verify(u => u.DrinkRepository.UpdateAsync(It.IsAny<Drink>()), Times.AtLeastOnce);

        }

        [Fact]
        public async Task UpdateDrink_BarNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var drinkId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var request = new DrinkRequest
            {
                BarId = barId,
                DrinkCategoryId = Guid.NewGuid(),
                Images = new List<IFormFile> { Mock.Of<IFormFile>() },
                OldImages = new List<string> { "image1.jpg" },
                DrinkBaseEmo = new List<Guid> { Guid.NewGuid() }
            };
            var account = new Account
            {
                AccountId = accountId,
                BarId = barId
            };
            // Mock authentication
            _authenticationMock.Setup(a => a.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);
            // Mock account repository
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByID(accountId))
                .Returns(account);
            // Mock bar repository to return empty list (bar not found)
            _unitOfWorkMock.Setup(u => u.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar>().AsQueryable());
            // Mock drink repository
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Drink> { new Drink { DrinkId = drinkId } });
            // Mock drink category repository
            _unitOfWorkMock.Setup(u => u.DrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<DrinkCategory> { new DrinkCategory() }.AsQueryable());
            // Mock emotional drink category repository
            _unitOfWorkMock.Setup(u => u.EmotionalDrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EmotionalDrinkCategory> { new EmotionalDrinkCategory() }.AsQueryable());
            // Mock mapper
            _mapperMock.Setup(m => m.Map<DrinkResponse>(It.IsAny<Drink>()))
                .Returns(new DrinkResponse());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkService.UpdateDrink(drinkId, request));

        }

        [Fact]
        public async Task UpdateDrink_UnauthorizedAccess_ShouldThrowUnAuthorizedException()
        {
            // Arrange
            var drinkId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid(); // Bar ID khác với request

            var request = new DrinkRequest
            {
                BarId = barId,
                DrinkCategoryId = Guid.NewGuid(),
                Images = new List<IFormFile> { Mock.Of<IFormFile>() },
                OldImages = new List<string> { "image1.jpg" },
                DrinkBaseEmo = new List<Guid> { Guid.NewGuid() }
            };
            var account = new Account
            {
                AccountId = accountId,
                BarId = differentBarId // Account thuộc về bar khác
            };
            var drink = new Drink { DrinkId = drinkId };
            // Mock authentication
            _authenticationMock.Setup(a => a.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);
            // Mock account repository - Account thuộc bar khác
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByID(accountId))
                .Returns(account);
            // Mock bar repository - Bar tồn tại
            _unitOfWorkMock.Setup(u => u.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE } }.AsQueryable());
            // Mock drink repository
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Drink> { drink });
            // Mock drink category repository
            _unitOfWorkMock.Setup(u => u.DrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<DrinkCategory> { new DrinkCategory() }.AsQueryable());
            // Mock emotional drink category repository
            _unitOfWorkMock.Setup(u => u.EmotionalDrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EmotionalDrinkCategory> { new EmotionalDrinkCategory() }.AsQueryable());
            // Mock drink emotional category repository
            _unitOfWorkMock.Setup(u => u.DrinkEmotionalCategoryRepository.GetAsync(
                It.IsAny<Expression<Func<DrinkEmotionalCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkEmotionalCategory>, IOrderedQueryable<DrinkEmotionalCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<DrinkEmotionalCategory>());
            // Mock mapper
            _mapperMock.Setup(m => m.Map(It.IsAny<DrinkRequest>(), It.IsAny<Drink>()))
                .Returns(drink);
            _mapperMock.Setup(m => m.Map<DrinkResponse>(It.IsAny<Drink>()))
                .Returns(new DrinkResponse());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _drinkService.UpdateDrink(drinkId, request));

        }

        [Fact]
        public async Task UpdateDrink_DrinkCategoryNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var drinkId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var request = new DrinkRequest
            {
                BarId = barId,
                DrinkCategoryId = Guid.NewGuid(),
                Images = new List<IFormFile> { Mock.Of<IFormFile>() },
                OldImages = new List<string> { "image1.jpg" },
                DrinkBaseEmo = new List<Guid> { Guid.NewGuid() }
            };
            var account = new Account
            {
                AccountId = accountId,
                BarId = barId
            };
            var drink = new Drink { DrinkId = drinkId };
            // Mock bar repository - Bar exists
            _unitOfWorkMock.Setup(u => u.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE } }.AsQueryable());
            // Mock authentication
            _authenticationMock.Setup(a => a.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);
            // Mock account repository
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByID(accountId))
                .Returns(account);
            // Mock drink repository - Drink exists
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Drink> { drink });
            // Mock drink category repository - Category not found
            _unitOfWorkMock.Setup(u => u.DrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<DrinkCategory>().AsQueryable());
            // Mock emotional drink category repository
            _unitOfWorkMock.Setup(u => u.EmotionalDrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EmotionalDrinkCategory> { new EmotionalDrinkCategory() }.AsQueryable());
            // Mock drink emotional category repository
            _unitOfWorkMock.Setup(u => u.DrinkEmotionalCategoryRepository.GetAsync(
                It.IsAny<Expression<Func<DrinkEmotionalCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkEmotionalCategory>, IOrderedQueryable<DrinkEmotionalCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<DrinkEmotionalCategory>());
            // Mock mapper
            _mapperMock.Setup(m => m.Map(It.IsAny<DrinkRequest>(), It.IsAny<Drink>()))
                .Returns(drink);
            _mapperMock.Setup(m => m.Map<DrinkResponse>(It.IsAny<Drink>()))
                .Returns(new DrinkResponse());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkService.UpdateDrink(drinkId, request));

        }

        [Fact]
        public async Task UpdateDrink_DrinkNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var drinkId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var request = new DrinkRequest
            {
                BarId = barId,
                DrinkCategoryId = Guid.NewGuid(),
                Images = new List<IFormFile> { Mock.Of<IFormFile>() },
                OldImages = new List<string> { "image1.jpg" }
            };
            var account = new Account
            {
                AccountId = accountId,
                BarId = barId
            };
            _unitOfWorkMock.Setup(u => u.BarRepository.Get(
               It.IsAny<Expression<Func<Bar, bool>>>(),
               It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .Returns(new List<Bar> { new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE } }.AsQueryable());
            _unitOfWorkMock.Setup(u => u.DrinkCategoryRepository.Get(
               It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
               It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .Returns(new List<DrinkCategory> { new DrinkCategory() }.AsQueryable());
            _authenticationMock.Setup(a => a.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
               .Returns(accountId);
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByID(accountId))
               .Returns(account);
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(new List<Drink>());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkService.UpdateDrink(drinkId, request));
        }

        [Fact]
        public async Task GetAllDrinkBasedEmoId_ValidRequest_ShouldReturnDrinkResponses()
        {
            // Arrange
            var emoId = Guid.NewGuid();
            var drinks = new List<Drink>
            {
                new Drink
                {
                    DrinkId = Guid.NewGuid(),
                    DrinkName = "Drink1",
                    DrinkCategory = new DrinkCategory { DrinksCategoryId = Guid.NewGuid() }
                },
                new Drink
                {
                    DrinkId = Guid.NewGuid(),
                    DrinkName = "Drink2",
                    DrinkCategory = new DrinkCategory { DrinksCategoryId = Guid.NewGuid() }
                }
            };
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(drinks);
            _mapperMock.Setup(m => m.Map<IEnumerable<DrinkResponse>>(It.IsAny<IEnumerable<Drink>>()))
               .Returns((IEnumerable<Drink> source) => source.Select(d => new DrinkResponse
               {
                   DrinkId = d.DrinkId,
                   DrinkName = d.DrinkName
               }));
            // Act
            var result = await _drinkService.GetAllDrinkBasedEmoId(emoId);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _unitOfWorkMock.Verify(u => u.DrinkRepository.GetAsync(
                It.IsAny<Expression<Func<Drink, bool>>>(),
                It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllDrinkBasedEmoId_NoData_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var emoId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(new List<Drink>());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkService.GetAllDrinkBasedEmoId(emoId));
            _unitOfWorkMock.Verify(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()),
               Times.Once);
        }

        [Fact]
        public async Task GetAllDrinkBasedEmoId_RepositoryError_ShouldThrowInternalServerErrorException()
        {
            // Arrange
            var emoId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ThrowsAsync(new CustomException.InternalServerErrorException("Database error"));
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _drinkService.GetAllDrinkBasedEmoId(emoId));
            _unitOfWorkMock.Verify(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()),
               Times.Once);
        }

        [Fact]
        public async Task GetAllDrinkCustomerOfBar_ValidRequest_ShouldReturnDrinkResponses()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var drinkCategoryId = Guid.NewGuid();
            var emotionalCategoryId = Guid.NewGuid();
            var drinks = new List<Drink>
   {
       new Drink
       {
           DrinkId = Guid.NewGuid(),
           DrinkName = "Drink1",
           Status = PrefixKeyConstant.TRUE,
           DrinkCategory = new DrinkCategory
           {
               DrinksCategoryId = drinkCategoryId,
               DrinksCategoryName = "Category1"
           },
           DrinkEmotionalCategories = new List<DrinkEmotionalCategory>
           {
               new DrinkEmotionalCategory
               {
                   EmotionalDrinkCategory = new EmotionalDrinkCategory
                   {
                       EmotionalDrinksCategoryId = emotionalCategoryId,
                       CategoryName = "Emotion1"
                   }
               }
           }
       },
       new Drink
       {
           DrinkId = Guid.NewGuid(),
           DrinkName = "Drink2",
           Status = PrefixKeyConstant.TRUE,
           DrinkCategory = new DrinkCategory
           {
               DrinksCategoryId = drinkCategoryId,
               DrinksCategoryName = "Category1"
           },
           DrinkEmotionalCategories = new List<DrinkEmotionalCategory>
           {
               new DrinkEmotionalCategory
               {
                   EmotionalDrinkCategory = new EmotionalDrinkCategory
                   {
                       EmotionalDrinksCategoryId = emotionalCategoryId,
                       CategoryName = "Emotion1"
                   }
               }
           }
       }
   };
            var expectedResponses = drinks.Select(d => new DrinkResponse
            {
                DrinkId = d.DrinkId,
                DrinkName = d.DrinkName,
                DrinkCategoryResponse = new DrinkCategoryResponse
                {
                    DrinksCategoryId = d.DrinkCategory.DrinksCategoryId.ToString(),
                    DrinksCategoryName = d.DrinkCategory.DrinksCategoryName
                },
                EmotionsDrink = d.DrinkEmotionalCategories.Select(e => new EmotionCategoryResponse
                {
                    EmotionalDrinksCategoryId = e.EmotionalDrinkCategory.EmotionalDrinksCategoryId,
                    CategoryName = e.EmotionalDrinkCategory.CategoryName
                }).ToList()
            });
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(drinks);
            _mapperMock.Setup(m => m.Map<IEnumerable<DrinkResponse>>(It.IsAny<IEnumerable<Drink>>()))
               .Returns(expectedResponses);
            // Act
            var result = await _drinkService.GetAllDrinkCustomerOfBar(barId);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            var firstDrink = result.First();
            Assert.NotNull(firstDrink.DrinkCategoryResponse);
            Assert.NotNull(firstDrink.EmotionsDrink);
            Assert.Single(firstDrink.EmotionsDrink);
            _unitOfWorkMock.Verify(u => u.DrinkRepository.GetAsync(
               It.Is<Expression<Func<Drink, bool>>>(x => x.Compile()(drinks[0])),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.Is<string>(i => i.Contains("DrinkCategory") && i.Contains("DrinkEmotionalCategories.EmotionalDrinkCategory")),
               It.IsAny<int?>(),
               It.IsAny<int?>()),
               Times.Once);
        }

        [Fact]
        public async Task GetAllDrinkCustomerOfBar_NoData_ShouldReturnEmptyList()
        {
            // Arrange
            var barId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ThrowsAsync(new CustomException.InternalServerErrorException("Database error"));
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _drinkService.GetAllDrinkCustomerOfBar(barId));
            _unitOfWorkMock.Verify(u => u.DrinkRepository.GetAsync(
               It.IsAny<Expression<Func<Drink, bool>>>(),
               It.IsAny<Func<IQueryable<Drink>, IOrderedQueryable<Drink>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()),
               Times.Once);
        }

    }
}
