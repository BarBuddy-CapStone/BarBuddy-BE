using Application.DTOs.Request.EmotionCategoryRequest;
using Application.DTOs.Response.EmotionCategory;
using Application.Service;
using AutoMapper;
using Domain.Common;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TorchSharp.Modules;

namespace UnitTests.Application.Services
{
    public class EmotionalDrinkCategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly EmotionalDrinkCategoryService _service;
        private readonly Mock<IGenericRepository<EmotionalDrinkCategory>> _emotionCategoryRepoMock;

        public EmotionalDrinkCategoryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _emotionCategoryRepoMock = new Mock<IGenericRepository<EmotionalDrinkCategory>>();

            _unitOfWorkMock.Setup(x => x.EmotionalDrinkCategoryRepository)
                .Returns(_emotionCategoryRepoMock.Object);

            _service = new EmotionalDrinkCategoryService(
                _unitOfWorkMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task GetEmotionCategory_ValidQuery_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 6,
                Search = "Test"
            };

            var emotionCategories = new List<EmotionalDrinkCategory>
            {
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = Guid.NewGuid(),
                    CategoryName = "Test Category 1",
                    IsDeleted = PrefixKeyConstant.FALSE
                },
                new EmotionalDrinkCategory
                {
                    EmotionalDrinksCategoryId = Guid.NewGuid(),
                    CategoryName = "Test Category 2",
                    IsDeleted = PrefixKeyConstant.FALSE
                }
            };

            var emotionCategoryResponses = emotionCategories.Select(x => new EmotionCategoryResponse
            {
                EmotionalDrinksCategoryId = x.EmotionalDrinksCategoryId,
                CategoryName = x.CategoryName
            }).ToList();

            // Setup repository Get
            _unitOfWorkMock.Setup(x => x.EmotionalDrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()
            )).Returns(emotionCategories);

            // Setup mapper
            _mapperMock.Setup(x => x.Map<List<EmotionCategoryResponse>>(It.IsAny<List<EmotionalDrinkCategory>>()))
                .Returns(emotionCategoryResponses);

            // Act
            var result = await _service.GetEmotionCategory(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(query.PageIndex, result.CurrentPage);
            Assert.Equal(query.PageSize, result.PageSize);
            Assert.Equal(emotionCategories.Count, result.TotalItems);
            Assert.Equal(1, result.TotalPages); // 2 items / 6 per page = 1 page
            Assert.Equal(2, result.EmotionCategoryResponses.Count);

            // Verify repository calls
            _unitOfWorkMock.Verify(x => x.EmotionalDrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()
            ), Times.Once);
        }

        [Fact]
        public async Task GetEmotionCategory_EmptyData_ThrowsDataNotFoundException()
        {
            // Arrange
            var query = new ObjectQueryCustom();
            _unitOfWorkMock.Setup(x => x.EmotionalDrinkCategoryRepository.Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EmotionalDrinkCategory>().AsQueryable());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() => _service.GetEmotionCategory(query));
        }

        [Fact]
        public async Task CreateEmotionCategory_ValidRequest_ReturnsCreatedCategory()
        {
            // Arrange
            var request = new CreateEmotionCategoryRequest
            {
                CategoryName = "Test Category"
            };

            var emotionCategory = new EmotionalDrinkCategory
            {
                EmotionalDrinksCategoryId = Guid.NewGuid(),
                CategoryName = request.CategoryName,
                IsDeleted = PrefixKeyConstant.FALSE
            };

            var expectedResponse = new EmotionCategoryResponse
            {
                EmotionalDrinksCategoryId = emotionCategory.EmotionalDrinksCategoryId,
                CategoryName = emotionCategory.CategoryName
            };

            // Setup repository check exists
            _unitOfWorkMock.Setup(x => x.EmotionalDrinkCategoryRepository.Exists(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>()))
                .Returns(false);

            // Setup mapper
            _mapperMock.Setup(x => x.Map<EmotionalDrinkCategory>(request))
                .Returns(emotionCategory);

            _mapperMock.Setup(x => x.Map<EmotionCategoryResponse>(emotionCategory))
                .Returns(expectedResponse);

            // Setup repository insert
            _unitOfWorkMock.Setup(x => x.EmotionalDrinkCategoryRepository.Insert(It.IsAny<EmotionalDrinkCategory>()))
                .Verifiable();

            _unitOfWorkMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateEmotionCategory(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.EmotionalDrinksCategoryId, result.EmotionalDrinksCategoryId);
            Assert.Equal(expectedResponse.CategoryName, result.CategoryName);

            // Verify repository calls
            _unitOfWorkMock.Verify(x => x.EmotionalDrinkCategoryRepository.Insert(It.IsAny<EmotionalDrinkCategory>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }
        [Fact]
        public async Task CreateEmotionCategory_DuplicateName_ThrowsInvalidDataException()
        {
            // Arrange
            var request = new CreateEmotionCategoryRequest { CategoryName = "Existing Category" };
            _unitOfWorkMock.Setup(x => x.EmotionalDrinkCategoryRepository.Exists(It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>()))
                .Returns(true);
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(() => _service.CreateEmotionCategory(request));
        }

        [Fact]
        public async Task DeleteEmotionCategory_ValidId_SoftDeletesCategory()
        {
            // Arrange
            var id = Guid.NewGuid();
            var category = new EmotionalDrinkCategory { EmotionalDrinksCategoryId = id, IsDeleted = false };

            _unitOfWorkMock.Setup(x => x.EmotionalDrinkCategoryRepository
            .Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
            .Returns(new List<EmotionalDrinkCategory> { category }.AsQueryable());
            // Act
            await _service.DeleteEmotionCategory(id);
            // Assert
            Assert.True(category.IsDeleted);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteEmotionCategory_InvalidId_ThrowsDataNotFoundException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _unitOfWorkMock.Setup(x => x.EmotionalDrinkCategoryRepository
                .Get(
                It.IsAny<Expression<Func<EmotionalDrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<EmotionalDrinkCategory>, IOrderedQueryable<EmotionalDrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<EmotionalDrinkCategory>().AsQueryable());
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(() => _service.DeleteEmotionCategory(id));
        }
    }
}
