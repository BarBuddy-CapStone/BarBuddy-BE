using Application.DTOs.DrinkCategory;
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
using static Domain.CustomException.CustomException;

namespace UnitTests.Application.Services
{
    public class DrinkCategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock; private readonly Mock<IMapper> _mapperMock;
        private readonly DrinkCategoryService _drinkCategoryService;
        private readonly Mock<IGenericRepository<DrinkCategory>> _drinkCategoryRepoMock;
        public DrinkCategoryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _drinkCategoryRepoMock = new Mock<IGenericRepository<DrinkCategory>>();

            _unitOfWorkMock.Setup(x => x.DrinkCategoryRepository)
                .Returns(_drinkCategoryRepoMock.Object);
            _drinkCategoryService = new DrinkCategoryService(
               _unitOfWorkMock.Object,
               _mapperMock.Object
           );
        }
        [Fact]
        public async Task CreateDrinkCategory_WhenValidRequest_ShouldReturnDrinkCategoryResponse()
        {
            // Arrange
            var request = new DrinkCategoryRequest
            {
                DrinksCategoryName = "Test Category"
            };
            var drinkCategory = new DrinkCategory
            {
                DrinksCategoryId = Guid.NewGuid(),
                DrinksCategoryName = request.DrinksCategoryName,
                IsDeleted = PrefixKeyConstant.FALSE
            };
            var expectedResponse = new DrinkCategoryResponse
            {
                DrinksCategoryId = drinkCategory.DrinksCategoryId.ToString(),
                DrinksCategoryName = drinkCategory.DrinksCategoryName
            };
            // Setup repository để trả về null (không tìm thấy tên category trùng)
            _drinkCategoryRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<DrinkCategory>());
            // Setup mapper
            _mapperMock.Setup(x => x.Map<DrinkCategory>(request))
                .Returns(drinkCategory);
            _mapperMock.Setup(x => x.Map<DrinkCategoryResponse>(drinkCategory))
                .Returns(expectedResponse);
            // Act
            var result = await _drinkCategoryService.CreateDrinkCategory(request);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.DrinksCategoryId, result.DrinksCategoryId);
            Assert.Equal(expectedResponse.DrinksCategoryName, result.DrinksCategoryName);
            // Verify repository calls
            _drinkCategoryRepoMock.Verify(x => x.InsertAsync(It.IsAny<DrinkCategory>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }
        [Fact]
        public async Task CreateDrinkCategory_WhenNameExists_ShouldThrowInvalidDataException()
        {
            // Arrange
            var request = new DrinkCategoryRequest
            {
                DrinksCategoryName = "Existing Category"
            };
            var existingCategory = new DrinkCategory
            {
                DrinksCategoryId = Guid.NewGuid(),
                DrinksCategoryName = request.DrinksCategoryName,
                IsDeleted = PrefixKeyConstant.FALSE
            };
            // Setup repository để trả về category đã tồn tại
            _drinkCategoryRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<DrinkCategory> { existingCategory });
            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _drinkCategoryService.CreateDrinkCategory(request));
            Assert.Equal("Tên thể loại đồ uống đã tồn tại, vui lòng thử lại", exception.Message);
            // Verify repository was not called for insert
            _drinkCategoryRepoMock.Verify(x => x.InsertAsync(It.IsAny<DrinkCategory>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
        }
        [Fact]
        public async Task CreateDrinkCategory_WhenInternalError_ShouldThrowInternalServerErrorException()
        {
            // Arrange
            var request = new DrinkCategoryRequest
            {
                DrinksCategoryName = "Test Category"
            };
            // Setup repository để throw exception
            _drinkCategoryRepoMock.Setup(x => x.Get(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Throws(new InternalServerErrorException("Database error"));
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _drinkCategoryService.CreateDrinkCategory(request));
            // Verify repository was not called for insert
            _drinkCategoryRepoMock.Verify(x => x.InsertAsync(It.IsAny<DrinkCategory>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteDrinkCategory_WhenValidId_ShouldReturnTrue()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var existingCategory = new DrinkCategory
            {
                DrinksCategoryId = categoryId,
                DrinksCategoryName = "Test Category",
                IsDeleted = PrefixKeyConstant.FALSE
            };
            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
               .ReturnsAsync(new List<DrinkCategory> { existingCategory });
            // Act
            var result = await _drinkCategoryService.DeleteDrinkCategory(categoryId);
            // Assert
            Assert.True(result);
            Assert.Equal(PrefixKeyConstant.TRUE, existingCategory.IsDeleted);

            // Verify repository calls
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteDrinkCategory_WhenCategoryNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _drinkCategoryRepoMock.Setup(x => x.GetByIdAsync(categoryId))
                .ReturnsAsync((DrinkCategory)null);
            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkCategoryService.DeleteDrinkCategory(categoryId));

            Assert.Equal("Không tìm thấy thể loại đồ uống!", exception.Message);

            // Verify repository was not called for update
            _drinkCategoryRepoMock.Verify(x => x.Update(It.IsAny<DrinkCategory>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteDrinkCategory_WhenInternalError_ShouldThrowInternalServerErrorException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new InternalServerErrorException("Database error"));
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _drinkCategoryService.DeleteDrinkCategory(categoryId));

            // Verify repository was not called for update
            _drinkCategoryRepoMock.Verify(x => x.Update(It.IsAny<DrinkCategory>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllDrinkCategory_WhenHaveData_ShouldReturnPaginatedResult()
        {
            // Arrange
            var query = new ObjectQueryCustom
            {
                PageIndex = 1,
                PageSize = 2
            };

            var drinkCategories = new List<DrinkCategory>
            {
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Category 1",
                    IsDeleted = PrefixKeyConstant.FALSE
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Category 2",
                    IsDeleted = PrefixKeyConstant.FALSE
                },
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Category 3",
                    IsDeleted = PrefixKeyConstant.FALSE
                }
            };

            var drinkCategoryResponses = drinkCategories.Select(x => new DrinkCategoryResponse
            {
                DrinksCategoryId = x.DrinksCategoryId.ToString(),
                DrinksCategoryName = x.DrinksCategoryName
            }).ToList();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(drinkCategories);

            _mapperMock.Setup(x => x.Map<List<DrinkCategoryResponse>>(It.IsAny<List<DrinkCategory>>()))
                .Returns(drinkCategoryResponses);

            // Act
            var result = await _drinkCategoryService.GetAllDrinkCategory(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(2, result.DrinkCategoryResponses.Count);
            Assert.Equal(3, result.TotalItems);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task GetAllDrinkCategory_WhenSearching_ShouldReturnFilteredResult()
        {
            // Arrange
            var query = new ObjectQueryCustom
            {
                Search = "Category 1",
                PageIndex = 1,
                PageSize = 6
            };

            var drinkCategories = new List<DrinkCategory>
            {
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Category 1",
                    IsDeleted = PrefixKeyConstant.FALSE
                }
            };

            var drinkCategoryResponses = drinkCategories.Select(x => new DrinkCategoryResponse
            {
                DrinksCategoryId = x.DrinksCategoryId.ToString(),
                DrinksCategoryName = x.DrinksCategoryName
            }).ToList();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(drinkCategories);

            _mapperMock.Setup(x => x.Map<List<DrinkCategoryResponse>>(It.IsAny<List<DrinkCategory>>()))
                .Returns(drinkCategoryResponses);

            // Act
            var result = await _drinkCategoryService.GetAllDrinkCategory(query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.DrinkCategoryResponses);
            Assert.Contains(result.DrinkCategoryResponses, x => x.DrinksCategoryName.Contains(query.Search));
        }

        [Fact]
        public async Task GetAllDrinkCategory_WhenNoData_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var query = new ObjectQueryCustom();
            var emptyList = new List<DrinkCategory>();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                 It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                 It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                 It.IsAny<string>(),
                 It.IsAny<int?>(),
                 It.IsAny<int?>()))
                 .ReturnsAsync(emptyList);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkCategoryService.GetAllDrinkCategory(query));

            Assert.Equal("Không tìm thấy thể loại đồ uống nào !", exception.Message);
        }

        [Fact]
        public async Task GetAllDrinkCategory_WhenInternalError_ShouldThrowInternalServerErrorException()
        {
            // Arrange
            var query = new ObjectQueryCustom();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _drinkCategoryService.GetAllDrinkCategory(query));
        }

        [Fact]
        public async Task GetAllDrinkCateOfBar_WhenValidBarId_ShouldReturnDrinkCategoryResponses()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var drinkCategories = new List<DrinkCategory>
            {
                new DrinkCategory
                {
                    DrinksCategoryId = Guid.NewGuid(),
                    DrinksCategoryName = "Category 1",
                    IsDeleted = PrefixKeyConstant.FALSE
                }
            };

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(drinkCategories);

            var expectedResponse = drinkCategories.Select(x => new DrinkCategoryResponse
            {
                DrinksCategoryId = x.DrinksCategoryId.ToString(),
                DrinksCategoryName = x.DrinksCategoryName
            });

            _mapperMock.Setup(x => x.Map<IEnumerable<DrinkCategoryResponse>>(drinkCategories))
                .Returns(expectedResponse);

            // Act
            var result = await _drinkCategoryService.GetAllDrinkCateOfBar(barId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedResponse.First().DrinksCategoryName, result.First().DrinksCategoryName);
        }

        [Fact]
        public async Task GetAllDrinkCateOfBar_WhenNoData_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var emptyList = new List<DrinkCategory>();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(emptyList);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkCategoryService.GetAllDrinkCateOfBar(barId));

            Assert.Equal("Không tìm thấy danh sách thể loại đồ uống nào của quán bar !", exception.Message);
        }

        [Fact]
        public async Task GetAllDrinkCateOfBar_WhenInternalError_ShouldThrowInternalServerErrorException()
        {
            // Arrange
            var barId = Guid.NewGuid();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _drinkCategoryService.GetAllDrinkCateOfBar(barId));
        }

        [Fact]
        public async Task GetDrinkCategoryById_WhenValidId_ShouldReturnDrinkCategoryResponse()
        {
            // Arrange
            var drinkCateId = Guid.NewGuid();
            var drinkCategory = new DrinkCategory
            {
                DrinksCategoryId = drinkCateId,
                DrinksCategoryName = "Test Category",
                IsDeleted = PrefixKeyConstant.FALSE
            };

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<DrinkCategory> { drinkCategory });

            var expectedResponse = new DrinkCategoryResponse
            {
                DrinksCategoryId = drinkCategory.DrinksCategoryId.ToString(),
                DrinksCategoryName = drinkCategory.DrinksCategoryName
            };

            _mapperMock.Setup(x => x.Map<DrinkCategoryResponse>(drinkCategory))
                .Returns(expectedResponse);

            // Act
            var result = await _drinkCategoryService.GetDrinkCategoryById(drinkCateId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.DrinksCategoryId, result.DrinksCategoryId);
            Assert.Equal(expectedResponse.DrinksCategoryName, result.DrinksCategoryName);
        }

        [Fact]
        public async Task GetDrinkCategoryById_WhenCategoryNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var drinkCateId = Guid.NewGuid();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<DrinkCategory>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkCategoryService.GetDrinkCategoryById(drinkCateId));

            Assert.Equal("The category drink is empty !", exception.Message);
        }

        [Fact]
        public async Task GetDrinkCategoryById_WhenInternalError_ShouldThrowInternalServerErrorException()
        {
            // Arrange
            var drinkCateId = Guid.NewGuid();

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _drinkCategoryService.GetDrinkCategoryById(drinkCateId));
        }

        [Fact]
        public async Task UpdateDrinkCategory_WhenValidId_ShouldReturnUpdatedDrinkCategoryResponse()
        {
            // Arrange
            var drinkCateId = Guid.NewGuid();
            var existingCategory = new DrinkCategory
            {
                DrinksCategoryId = drinkCateId,
                DrinksCategoryName = "Old Category",
                IsDeleted = PrefixKeyConstant.FALSE
            };

            var request = new UpdDrinkCategoryRequest
            {
                Description = "Updated Description"
            };

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<DrinkCategory> { existingCategory });

            _mapperMock.Setup(x => x.Map(request, existingCategory))
                .Returns(existingCategory);

            var expectedResponse = new DrinkCategoryResponse
            {
                DrinksCategoryId = existingCategory.DrinksCategoryId.ToString(),
                DrinksCategoryName = existingCategory.DrinksCategoryName
            };

            _mapperMock.Setup(x => x.Map<DrinkCategoryResponse>(existingCategory))
                .Returns(expectedResponse);

            // Act
            var result = await _drinkCategoryService.UpdateDrinkCategory(drinkCateId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.DrinksCategoryId, result.DrinksCategoryId);
            Assert.Equal(expectedResponse.DrinksCategoryName, result.DrinksCategoryName);
            _unitOfWorkMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateDrinkCategory_WhenCategoryNotFound_ShouldThrowDataNotFoundException()
        {
            // Arrange
            var drinkCateId = Guid.NewGuid();
            var request = new UpdDrinkCategoryRequest
            {
                Description = "Updated Description"
            };

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<DrinkCategory>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _drinkCategoryService.UpdateDrinkCategory(drinkCateId, request));

            Assert.Equal("Không tìm thấy thể loại đồ uống !", exception.Message);
        }

        [Fact]
        public async Task UpdateDrinkCategory_WhenInternalError_ShouldThrowInternalServerErrorException()
        {
            // Arrange
            var drinkCateId = Guid.NewGuid();
            var request = new UpdDrinkCategoryRequest
            {
                Description = "Updated Description"
            };

            _drinkCategoryRepoMock.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<DrinkCategory, bool>>>(),
                It.IsAny<Func<IQueryable<DrinkCategory>, IOrderedQueryable<DrinkCategory>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _drinkCategoryService.UpdateDrinkCategory(drinkCateId, request));
        }
    }
}
