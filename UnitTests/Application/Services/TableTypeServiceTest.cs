using Application.DTOs.TableType;
using Application.Interfaces;
using Application.Service;
using AutoMapper;
using Domain.Common;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;

namespace UnitTests.Application.Services
{
    public class TableTypeServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAuthentication> _mockAuthentication;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly TableTypeService _tableTypeService;

        public TableTypeServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockAuthentication = new Mock<IAuthentication>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            
            _tableTypeService = new TableTypeService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockAuthentication.Object,
                _mockHttpContextAccessor.Object
            );
        }

        [Fact]
        public async Task CreateTableType_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var request = new TableTypeRequest
            {
                BarId = barId,
                TypeName = "VIP Table",
                Description = "VIP Table Description",
                MinimumGuest = 2,
                MaximumGuest = 4,
                MinimumPrice = 1000000
            };

            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            // Setup mock cho TableTypeRepository.InsertAsync
            _mockUnitOfWork.Setup(x => x.TableTypeRepository.InsertAsync(It.IsAny<TableType>()))
                .Returns(Task.CompletedTask);

            // Act
            await _tableTypeService.CreateTableType(request);

            // Assert
            _mockUnitOfWork.Verify(x => x.TableTypeRepository.InsertAsync(It.Is<TableType>(tt => 
                tt.BarId == barId &&
                tt.TypeName == request.TypeName &&
                tt.Description == request.Description &&
                tt.MinimumGuest == request.MinimumGuest &&
                tt.MaximumGuest == request.MaximumGuest &&
                tt.MinimumPrice == request.MinimumPrice &&
                tt.IsDeleted == false
            )), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateTableType_BarNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var request = new TableTypeRequest { BarId = barId };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableTypeService.CreateTableType(request)
            );
        }

        [Fact]
        public async Task CreateTableType_UnauthorizedAccess_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();
            
            var request = new TableTypeRequest { BarId = barId };
            var account = new Account { AccountId = accountId, BarId = differentBarId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tableTypeService.CreateTableType(request)
            );
        }

        [Fact]
        public async Task CreateTableType_InvalidGuestNumbers_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            
            var request = new TableTypeRequest 
            { 
                BarId = barId,
                MinimumGuest = 4,
                MaximumGuest = 2
            };
            
            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _tableTypeService.CreateTableType(request)
            );
        }

        [Fact]
        public async Task DeleteTableType_Success_WhenNoTablesUsingIt()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();

            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };
            var tableType = new TableType 
            { 
                TableTypeId = tableTypeId,
                BarId = barId,
                IsDeleted = false
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType> { tableType });

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table>());

            // Act
            var result = await _tableTypeService.DeleteTableType(tableTypeId);

            // Assert
            Assert.True(result);
            _mockUnitOfWork.Verify(x => x.TableTypeRepository.UpdateAsync(It.Is<TableType>(tt => 
                tt.TableTypeId == tableTypeId && 
                tt.IsDeleted == true)), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTableType_ReturnsFalse_WhenTablesAreUsingIt()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();

            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };
            var tableType = new TableType 
            { 
                TableTypeId = tableTypeId,
                BarId = barId,
                IsDeleted = false
            };
            var table = new Table { TableTypeId = tableTypeId, IsDeleted = false };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType> { tableType });

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
               It.IsAny<Expression<Func<Table, bool>>>(),
               It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
               It.IsAny<string>(),
               It.IsAny<int?>(),
               It.IsAny<int?>()))
               .ReturnsAsync(new List<Table> { table });

            // Act
            var result = await _tableTypeService.DeleteTableType(tableTypeId);

            // Assert
            Assert.False(result);
            _mockUnitOfWork.Verify(x => x.TableTypeRepository.UpdateAsync(It.IsAny<TableType>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteTableType_ThrowsException_WhenTableTypeNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(new Account());

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableTypeService.DeleteTableType(tableTypeId)
            );
        }

        [Fact]
        public async Task DeleteTableType_ThrowsException_WhenBarNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();

            var account = new Account { AccountId = accountId, BarId = barId };
            var tableType = new TableType 
            { 
                TableTypeId = tableTypeId,
                BarId = barId,
                IsDeleted = false
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
            It.IsAny<Expression<Func<TableType, bool>>>(),
            It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType> { tableType });

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableTypeService.DeleteTableType(tableTypeId)
            );
        }

        [Fact]
        public async Task DeleteTableType_ThrowsException_WhenUnauthorizedAccess()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();

            var account = new Account { AccountId = accountId, BarId = differentBarId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };
            var tableType = new TableType 
            { 
                TableTypeId = tableTypeId,
                BarId = barId,
                IsDeleted = false
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType> { tableType });

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tableTypeService.DeleteTableType(tableTypeId)
            );
        }

        [Fact]
        public async Task GetAll_Success_ReturnsTableTypes()
        {
            // Arrange
            var tableTypes = new List<TableType>
            {
                new TableType 
                { 
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "VIP Table",
                    MinimumPrice = 2000000,
                    IsDeleted = false
                },
                new TableType 
                { 
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "Normal Table",
                    MinimumPrice = 1000000,
                    IsDeleted = false
                }
            };

            var expectedResponses = new List<TableTypeResponse>
            {
                new TableTypeResponse 
                { 
                    TableTypeId = tableTypes[0].TableTypeId,
                    TypeName = tableTypes[0].TypeName,
                    MinimumPrice = tableTypes[0].MinimumPrice
                },
                new TableTypeResponse 
                { 
                    TableTypeId = tableTypes[1].TableTypeId,
                    TypeName = tableTypes[1].TypeName,
                    MinimumPrice = tableTypes[1].MinimumPrice
                }
            };

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tableTypes);

            _mockMapper.Setup(x => x.Map<TableTypeResponse>(It.IsAny<TableType>()))
                .Returns<TableType>(tt => new TableTypeResponse 
                { 
                    TableTypeId = tt.TableTypeId,
                    TypeName = tt.TypeName,
                    MinimumPrice = tt.MinimumPrice
                });

            // Act
            var result = await _tableTypeService.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedResponses[0].TableTypeId, result[0].TableTypeId);
            Assert.Equal(expectedResponses[0].TypeName, result[0].TypeName);
            Assert.Equal(expectedResponses[0].MinimumPrice, result[0].MinimumPrice);
            Assert.Equal(expectedResponses[1].TableTypeId, result[1].TableTypeId);
            Assert.Equal(expectedResponses[1].TypeName, result[1].TypeName);
            Assert.Equal(expectedResponses[1].MinimumPrice, result[1].MinimumPrice);
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_WhenNoTableTypes()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType>());

            // Act
            var result = await _tableTypeService.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAll_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tableTypeService.GetAll()
            );
        }

        [Fact]
        public async Task GetAllForAdmin_Status0_ReturnsAllTableTypes()
        {
            // Arrange
            var tableTypes = new List<TableType>
            {
                new TableType 
                { 
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "VIP Table",
                    MinimumPrice = 2000000,
                    IsDeleted = false
                },
                new TableType 
                { 
                    TableTypeId = Guid.NewGuid(),
                    TypeName = "Normal Table",
                    MinimumPrice = 1000000,
                    IsDeleted = false
                }
            };

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tableTypes);

            _mockMapper.Setup(x => x.Map<TableTypeResponse>(It.IsAny<TableType>()))
                .Returns<TableType>(tt => new TableTypeResponse 
                { 
                    TableTypeId = tt.TableTypeId,
                    TypeName = tt.TypeName,
                    MinimumPrice = tt.MinimumPrice
                });

            // Act
            var result = await _tableTypeService.GetAllForAdmin(0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllForAdmin_Status1_ReturnsTableTypesWithTables()
        {
            // Arrange
            var tableTypeId = Guid.NewGuid();
            var tableTypes = new List<TableType>
            {
                new TableType 
                { 
                    TableTypeId = tableTypeId,
                    TypeName = "VIP Table",
                    IsDeleted = false
                }
            };

            var tables = new List<Table>
            {
                new Table { TableTypeId = tableTypeId, IsDeleted = false }
            };

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tableTypes);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tables);

            _mockMapper.Setup(x => x.Map<TableTypeResponse>(It.IsAny<TableType>()))
                .Returns<TableType>(tt => new TableTypeResponse 
                { 
                    TableTypeId = tt.TableTypeId,
                    TypeName = tt.TypeName
                });

            // Act
            var result = await _tableTypeService.GetAllForAdmin(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllForAdmin_Status2_ReturnsTableTypesWithoutTables()
        {
            // Arrange
            var tableTypeId = Guid.NewGuid();
            var tableTypes = new List<TableType>
            {
                new TableType 
                { 
                    TableTypeId = tableTypeId,
                    TypeName = "VIP Table",
                    IsDeleted = false
                }
            };

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tableTypes);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table>());

            _mockMapper.Setup(x => x.Map<TableTypeResponse>(It.IsAny<TableType>()))
                .Returns<TableType>(tt => new TableTypeResponse 
                { 
                    TableTypeId = tt.TableTypeId,
                    TypeName = tt.TypeName
                });

            // Act
            var result = await _tableTypeService.GetAllForAdmin(2);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllForAdmin_InvalidStatus_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _tableTypeService.GetAllForAdmin(99)
            );
        }

        [Fact]
        public async Task GetById_Success_ReturnsTableType()
        {
            // Arrange
            var tableTypeId = Guid.NewGuid();
            var tableType = new TableType 
            { 
                TableTypeId = tableTypeId,
                TypeName = "VIP Table",
                Description = "VIP Table Description",
                MinimumGuest = 2,
                MaximumGuest = 4,
                MinimumPrice = 1000000,
                IsDeleted = false
            };

            var expectedResponse = new TableTypeResponse 
            { 
                TableTypeId = tableTypeId,
                TypeName = "VIP Table",
                Description = "VIP Table Description",
                MinimumGuest = 2,
                MaximumGuest = 4,
                MinimumPrice = 1000000
            };

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType> { tableType });

            _mockMapper.Setup(x => x.Map<TableTypeResponse>(It.IsAny<TableType>()))
                .Returns(expectedResponse);

            // Act
            var result = await _tableTypeService.GetById(tableTypeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.TableTypeId, result.TableTypeId);
            Assert.Equal(expectedResponse.TypeName, result.TypeName);
            Assert.Equal(expectedResponse.Description, result.Description);
            Assert.Equal(expectedResponse.MinimumGuest, result.MinimumGuest);
            Assert.Equal(expectedResponse.MaximumGuest, result.MaximumGuest);
            Assert.Equal(expectedResponse.MinimumPrice, result.MinimumPrice);
        }

        [Fact]
        public async Task GetById_TableTypeNotFound_ThrowsException()
        {
            // Arrange
            var tableTypeId = Guid.NewGuid();

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableTypeService.GetById(tableTypeId)
            );
        }

        [Fact]
        public async Task GetById_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var tableTypeId = Guid.NewGuid();

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tableTypeService.GetById(tableTypeId)
            );
        }

        [Fact]
        public async Task UpdateTableType_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();

            var request = new TableTypeRequest
            {
                BarId = barId,
                TypeName = "Updated VIP Table",
                Description = "Updated Description",
                MinimumGuest = 2,
                MaximumGuest = 4,
                MinimumPrice = 1500000
            };

            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };
            var existingTableType = new TableType 
            { 
                TableTypeId = tableTypeId,
                BarId = barId,
                IsDeleted = false
            };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByIdAsync(tableTypeId))
                .ReturnsAsync(existingTableType);

            // Act
            await _tableTypeService.UpdateTableType(request, tableTypeId);

            // Assert
            _mockUnitOfWork.Verify(x => x.TableTypeRepository.UpdateAsync(It.Is<TableType>(tt =>
                tt.TableTypeId == tableTypeId &&
                tt.TypeName == request.TypeName &&
                tt.Description == request.Description &&
                tt.MinimumGuest == request.MinimumGuest &&
                tt.MaximumGuest == request.MaximumGuest &&
                tt.MinimumPrice == request.MinimumPrice
            )), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTableType_TableTypeNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();
            var request = new TableTypeRequest { BarId = barId };

            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByIdAsync(tableTypeId))
                .ReturnsAsync((TableType)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableTypeService.UpdateTableType(request, tableTypeId)
            );
        }

        [Fact]
        public async Task UpdateTableType_UnauthorizedAccess_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();

            var request = new TableTypeRequest { BarId = barId };
            var account = new Account { AccountId = accountId, BarId = differentBarId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };
            var existingTableType = new TableType { TableTypeId = tableTypeId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByIdAsync(tableTypeId))
                .ReturnsAsync(existingTableType);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tableTypeService.UpdateTableType(request, tableTypeId)
            );
        }

        [Fact]
        public async Task UpdateTableType_InvalidGuestNumbers_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var tableTypeId = Guid.NewGuid();

            var request = new TableTypeRequest 
            { 
                BarId = barId,
                MinimumGuest = 4,
                MaximumGuest = 2
            };

            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };
            var existingTableType = new TableType { TableTypeId = tableTypeId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByIdAsync(tableTypeId))
                .ReturnsAsync(existingTableType);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _tableTypeService.UpdateTableType(request, tableTypeId)
            );
        }

        [Fact]
        public async Task GetAllTTOfBar_Success_ReturnsPaginatedResult()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var query = new ObjectQuery { PageIndex = 1, PageSize = 2 };

            var tableTypes = new List<TableType>
            {
                new TableType 
                { 
                    TableTypeId = Guid.NewGuid(),
                    BarId = barId,
                    TypeName = "VIP Table",
                    MinimumPrice = 2000000,
                    IsDeleted = false
                },
                new TableType 
                { 
                    TableTypeId = Guid.NewGuid(),
                    BarId = barId,
                    TypeName = "Normal Table",
                    MinimumPrice = 1000000,
                    IsDeleted = false
                },
                new TableType 
                { 
                    TableTypeId = Guid.NewGuid(),
                    BarId = barId,
                    TypeName = "Standard Table",
                    MinimumPrice = 500000,
                    IsDeleted = false
                }
            };

            var expectedResponses = tableTypes.Take(2).Select(tt => new TableTypeResponse 
            { 
                TableTypeId = tt.TableTypeId,
                TypeName = tt.TypeName,
                MinimumPrice = tt.MinimumPrice
            }).ToList();

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tableTypes);

            _mockMapper.Setup(x => x.Map<List<TableTypeResponse>>(It.IsAny<List<TableType>>()))
                .Returns(expectedResponses);

            // Act
            var result = await _tableTypeService.GetAllTTOfBar(barId, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TableTypeResponses.Count);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(3, result.TotalItems);
        }

        [Fact]
        public async Task GetAllTTOfBar_ReturnsEmptyList_WhenNoTableTypes()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var query = new ObjectQuery { PageIndex = 1, PageSize = 6 };

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<TableType>());

            _mockMapper.Setup(x => x.Map<List<TableTypeResponse>>(It.IsAny<List<TableType>>()))
                .Returns(new List<TableTypeResponse>());

            // Act
            var result = await _tableTypeService.GetAllTTOfBar(barId, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.TableTypeResponses);
            Assert.Equal(6, result.PageSize);
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(0, result.TotalPages);
            Assert.Equal(0, result.TotalItems);
        }

        [Fact]
        public async Task GetAllTTOfBar_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var query = new ObjectQuery { PageIndex = 1, PageSize = 6 };

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetAsync(
                It.IsAny<Expression<Func<TableType, bool>>>(),
                It.IsAny<Func<IQueryable<TableType>, IOrderedQueryable<TableType>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tableTypeService.GetAllTTOfBar(barId, query)
            );
        }
    }
}
