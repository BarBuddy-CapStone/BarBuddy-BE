using Application.DTOs.BarTime;
using Application.DTOs.Table;
using Application.Interfaces;
using Application.Service;
using AutoMapper;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Linq.Expressions;

namespace UnitTests.Application.Services
{
    public class TableServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuthentication> _mockAuthentication;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IMapper> _mockMapper;
        private readonly TableService _tableService;
        public TableServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuthentication = new Mock<IAuthentication>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockMapper = new Mock<IMapper>();
            _tableService = new TableService(
                _mockUnitOfWork.Object,
                _mockAuthentication.Object,
                _mockHttpContextAccessor.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task CreateTable_Success()
        {
            // Arrange
            var request = new CreateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Test Table",
                Status = 1
            };

            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = barId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table>());

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            // Act
            await _tableService.CreateTable(request);

            // Assert
            _mockUnitOfWork.Verify(x => x.TableRepository.InsertAsync(It.IsAny<Table>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateTable_TableTypeNotFound_ThrowsException()
        {
            // Arrange
            var request = new CreateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Test Table",
                Status = 1
            };

            var accountId = Guid.NewGuid();
            var account = new Account { AccountId = accountId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns((TableType)null);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableService.CreateTable(request)
            );
        }

        [Fact]
        public async Task CreateTable_DuplicateTableName_ThrowsException()
        {
            // Arrange
            var request = new CreateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Test Table",
                Status = 1
            };

            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            var existingTable = new Table { TableName = request.TableName };
            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table> { existingTable });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataExistException>(
                () => _tableService.CreateTable(request)
            );
        }

        [Fact]
        public async Task CreateTable_BarNotFound_ThrowsException()
        {
            // Arrange
            var request = new CreateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Test Table",
                Status = 1
            };

            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();

            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table>());

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableService.CreateTable(request)
            );
        }

        [Fact]
        public async Task CreateTable_UnauthorizedAccess_ThrowsException()
        {
            // Arrange
            var request = new CreateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Test Table",
                Status = 1
            };

            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();

            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = differentBarId };
            var bar = new Bar { BarId = barId, Status = PrefixKeyConstant.TRUE };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table>());

            _mockUnitOfWork.Setup(x => x.BarRepository.Get(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Bar> { bar }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tableService.CreateTable(request)
            );
        }

        [Fact]
        public async Task DeleteTable_Success()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var table = new Table { TableId = tableId, IsDeleted = false };

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ReturnsAsync(table);

            _mockUnitOfWork.Setup(x => x.BookingTableRepository.GetAsync(
                It.IsAny<Expression<Func<BookingTable, bool>>>(),
                It.IsAny<Func<IQueryable<BookingTable>, IOrderedQueryable<BookingTable>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<BookingTable>());

            // Act
            var result = await _tableService.DeleteTable(tableId);

            // Assert
            Assert.True(result);
            Assert.True(table.IsDeleted);
            _mockUnitOfWork.Verify(x => x.TableRepository.UpdateAsync(table), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTable_WithFutureBookings_ReturnsFalse()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var table = new Table { TableId = tableId, IsDeleted = false };
            var futureBooking = new BookingTable 
            { 
                TableId = tableId,
                Booking = new Booking { BookingDate = DateTime.Now.AddDays(1) }
            };

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ReturnsAsync(table);

            _mockUnitOfWork.Setup(x => x.BookingTableRepository.GetAsync(
                It.IsAny<Expression<Func<BookingTable, bool>>>(),
                It.IsAny<Func<IQueryable<BookingTable>, IOrderedQueryable<BookingTable>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<BookingTable> { futureBooking });

            // Act
            var result = await _tableService.DeleteTable(tableId);

            // Assert
            Assert.False(result);
            Assert.False(table.IsDeleted);
            _mockUnitOfWork.Verify(x => x.TableRepository.UpdateAsync(It.IsAny<Table>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteTable_TableNotFound_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ReturnsAsync((Table)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableService.DeleteTable(tableId)
            );
        }

        [Fact]
        public async Task DeleteTable_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var tableId = Guid.NewGuid();

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tableService.DeleteTable(tableId)
            );
        }

        [Fact]
        public async Task GetAll_Success_WithPagination()
        {
            // Arrange
            var tableTypeId = Guid.NewGuid();
            var tables = new List<Table>
            {
                new Table { TableId = Guid.NewGuid(), TableName = "Table 1", Status = 1, IsDeleted = false,
                    TableType = new TableType { MinimumGuest = 2, MaximumGuest = 4, MinimumPrice = 100000, TypeName = "Type 1" } },
                new Table { TableId = Guid.NewGuid(), TableName = "Table 2", Status = 1, IsDeleted = false,
                    TableType = new TableType { MinimumGuest = 4, MaximumGuest = 6, MinimumPrice = 200000, TypeName = "Type 2" } },
                new Table { TableId = Guid.NewGuid(), TableName = "Table 3", Status = 1, IsDeleted = false,
                    TableType = new TableType { MinimumGuest = 6, MaximumGuest = 8, MinimumPrice = 300000, TypeName = "Type 3" } }
            };

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tables);

            // Act
            var result = await _tableService.GetAll(tableTypeId, "Table", 1, 1, 2);

            // Assert
            Assert.NotNull(result.response);
            Assert.Equal(2, result.TotalPage);
            Assert.Equal(3, tables.Count);
        }

        [Fact]
        public async Task GetAll_Success_WithFilter()
        {
            // Arrange
            var tableTypeId = Guid.NewGuid();
            var filteredTables = new List<Table>
            {
                new Table { 
                    TableId = Guid.NewGuid(), 
                    TableName = "VIP Table", 
                    Status = 1, 
                    IsDeleted = false,
                    TableType = new TableType { 
                        MinimumGuest = 2, 
                        MaximumGuest = 4, 
                        MinimumPrice = 100000, 
                        TypeName = "VIP" 
                    } 
                }
            };

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(filteredTables);

            // Act
            var result = await _tableService.GetAll(tableTypeId, "VIP", 1, 1, 10);

            // Assert
            Assert.NotNull(result.response);
            Assert.Single(result.response);
            Assert.Equal(1, result.TotalPage);
            Assert.Contains(result.response, t => t.TableName == "VIP Table");
        }

        [Fact]
        public async Task GetAll_ReturnsEmpty_WhenNoTables()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table>());

            // Act
            var result = await _tableService.GetAll(null, null, null, 1, 10);

            // Assert
            Assert.Empty(result.response);
            Assert.Equal(1, result.TotalPage);
        }

        [Fact]
        public async Task GetAll_Success_WithLessItemsThanPageSize()
        {
            // Arrange
            var tables = new List<Table>
            {
                new Table { 
                    TableId = Guid.NewGuid(), 
                    TableName = "Table 1", 
                    Status = 1, 
                    IsDeleted = false,
                    TableType = new TableType { 
                        MinimumGuest = 2, 
                        MaximumGuest = 4, 
                        MinimumPrice = 100000, 
                        TypeName = "Type 1" 
                    } 
                }
            };

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tables);

            // Act
            var result = await _tableService.GetAll(null, null, null, 1, 10);

            // Assert
            Assert.Single(result.response);
            Assert.Equal(1, result.TotalPage);
        }

        [Fact]
        public async Task GetAll_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tableService.GetAll(null, null, null, 1, 10)
            );
        }

        [Fact]
        public async Task GetAllOfBar_Success()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var requestDate = DateTime.Now.AddDays(1);
            var requestTime = new TimeSpan(20, 0, 0); // 8:00 PM

            var bar = new Bar 
            { 
                BarId = barId,
                TimeSlot = 2.0,
                BarTimes = new List<BarTime> 
                { 
                    new BarTime 
                    { 
                        DayOfWeek = (int)requestDate.DayOfWeek,
                        StartTime = new TimeSpan(18, 0, 0), // 6:00 PM
                        EndTime = new TimeSpan(23, 0, 0)    // 11:00 PM
                    } 
                }
            };

            var tableId = Guid.NewGuid();
            var tables = new List<Table>
            {
                new Table 
                { 
                    TableId = tableId,
                    TableName = "Table 1",
                    Status = 1,
                    IsDeleted = false,
                    TableType = new TableType 
                    { 
                        Bar = bar,
                        MinimumGuest = 2,
                        MaximumGuest = 4,
                        MinimumPrice = 100000,
                        TypeName = "Type 1"
                    }
                }
            };

            _mockUnitOfWork.Setup(x => x.BookingTableRepository.GetAsync(
                It.IsAny<Expression<Func<BookingTable, bool>>>(),
                It.IsAny<Func<IQueryable<BookingTable>, IOrderedQueryable<BookingTable>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<BookingTable>());

            var barTimeResponses = new List<BarTimeResponse>
            {
                new BarTimeResponse 
                { 
                    DayOfWeek = (int)requestDate.DayOfWeek,
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(23, 0, 0)
                }
            };

            _mockUnitOfWork.Setup(x => x.BarRepository.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar> { bar });

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tables);

            _mockMapper.Setup(x => x.Map<List<BarTimeResponse>>(It.IsAny<List<BarTime>>()))
                .Returns(barTimeResponses);

            // Act
            var result = await _tableService.GetAllOfBar(barId, null, null, null, 1, 10, requestDate, requestTime);

            // Assert
            Assert.NotNull(result.response);
            Assert.Equal(1, result.TotalPage);
            Assert.NotNull(result.barTimes);
            Assert.Equal(2.0, result.timeSlot);
            Assert.Single(result.response);
            Assert.Equal(0, result.response.First().Status);
        }

        [Fact]
        public async Task GetAllOfBar_BarNotFound_ThrowsException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var requestDate = DateTime.Now.AddDays(1);
            var requestTime = new TimeSpan(20, 0, 0);

            _mockUnitOfWork.Setup(x => x.BarRepository.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar>());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableService.GetAllOfBar(barId, null, null, null, 1, 10, requestDate, requestTime)
            );
        }

        [Fact]
        public async Task GetAllOfBar_InvalidDate_ThrowsException()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var requestDate = DateTime.Now.AddDays(-1); // Past date
            var requestTime = new TimeSpan(20, 0, 0);

            var bar = new Bar
            {
                BarId = barId,
                TimeSlot = 2.0,
                BarTimes = new List<BarTime>
                {
                    new BarTime
                    {
                        DayOfWeek = (int)requestDate.DayOfWeek,
                        StartTime = new TimeSpan(18, 0, 0),
                        EndTime = new TimeSpan(23, 0, 0)
                    }
                }
            };

            _mockUnitOfWork.Setup(x => x.BarRepository.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar> { bar });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _tableService.GetAllOfBar(barId, null, null, null, 1, 10, requestDate, requestTime)
            );
        }

        [Fact]
        public async Task GetAllOfBar_InvalidTime_ReturnsTablesWithUnavailableStatus()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var requestDate = DateTime.Now.AddDays(1);
            var requestTime = new TimeSpan(15, 0, 0); // 3:00 PM - Outside bar hours

            var bar = new Bar 
            { 
                BarId = barId,
                TimeSlot = 2.0,
                BarTimes = new List<BarTime> 
                { 
                    new BarTime 
                    { 
                        DayOfWeek = (int)requestDate.DayOfWeek,
                        StartTime = new TimeSpan(18, 0, 0), // 6:00 PM
                        EndTime = new TimeSpan(23, 0, 0)    // 11:00 PM
                    } 
                }
            };

            var tables = new List<Table>
            {
                new Table 
                { 
                    TableId = Guid.NewGuid(),
                    TableName = "Table 1",
                    Status = 1,
                    IsDeleted = false,
                    TableType = new TableType { Bar = bar }
                }
            };

            _mockUnitOfWork.Setup(x => x.BarRepository.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Bar> { bar });

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(tables);

            _mockMapper.Setup(x => x.Map<List<BarTimeResponse>>(It.IsAny<List<BarTime>>()))
                .Returns(new List<BarTimeResponse>());

            // Act
            var result = await _tableService.GetAllOfBar(barId, null, null, null, 1, 10, requestDate, requestTime);

            // Assert
            Assert.NotNull(result.response);
            Assert.All(result.response, table => Assert.Equal(3, table.Status)); // Status 3 = Unavailable
        }

        [Fact]
        public async Task GetAllOfBar_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var barId = Guid.NewGuid();
            var requestDate = DateTime.Now.AddDays(1);
            var requestTime = new TimeSpan(20, 0, 0);

            _mockUnitOfWork.Setup(x => x.BarRepository.GetAsync(
                It.IsAny<Expression<Func<Bar, bool>>>(),
                It.IsAny<Func<IQueryable<Bar>, IOrderedQueryable<Bar>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tableService.GetAllOfBar(barId, null, null, null, 1, 10, requestDate, requestTime)
            );
        }

        [Fact]
        public async Task UpdateTable_Success()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var request = new UpdateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Updated Table",
                Status = 1
            };

            var existingTable = new Table 
            { 
                TableId = tableId, 
                TableName = "Old Table",
                TableTypeId = request.TableTypeId,
                IsDeleted = false 
            };
            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ReturnsAsync(existingTable);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table>());

            // Act
            await _tableService.UpdateTable(tableId, request);

            // Assert
            Assert.Equal(request.TableName, existingTable.TableName);
            Assert.Equal(request.Status, existingTable.Status);
            _mockUnitOfWork.Verify(x => x.TableRepository.UpdateAsync(existingTable), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTable_TableTypeNotFound_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var request = new UpdateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Updated Table",
                Status = 1
            };

            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns((TableType)null);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableService.UpdateTable(tableId, request)
            );
        }

        [Fact]
        public async Task UpdateTable_TableNotFound_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var request = new UpdateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Updated Table",
                Status = 1
            };

            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ReturnsAsync((Table)null);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableService.UpdateTable(tableId, request)
            );
        }

        [Fact]
        public async Task UpdateTable_InvalidTableType_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var request = new UpdateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Updated Table",
                Status = 1
            };

            var existingTable = new Table 
            { 
                TableId = tableId, 
                TableTypeId = Guid.NewGuid(), // Different TableTypeId
                IsDeleted = false 
            };
            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ReturnsAsync(existingTable);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InvalidDataException>(
                () => _tableService.UpdateTable(tableId, request)
            );
        }

        [Fact]
        public async Task UpdateTable_UnauthorizedAccess_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();
            var request = new UpdateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Updated Table",
                Status = 1
            };

            var existingTable = new Table 
            { 
                TableId = tableId, 
                TableTypeId = request.TableTypeId,
                IsDeleted = false 
            };
            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = differentBarId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ReturnsAsync(existingTable);

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tableService.UpdateTable(tableId, request)
            );
        }

        [Fact]
        public async Task UpdateTable_DuplicateTableName_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var request = new UpdateTableRequest
            {
                TableTypeId = Guid.NewGuid(),
                TableName = "Duplicate Table",
                Status = 1
            };

            var existingTable = new Table 
            { 
                TableId = tableId, 
                TableName = "Original Table",
                TableTypeId = request.TableTypeId,
                IsDeleted = false 
            };
            var duplicateTable = new Table { TableName = request.TableName };
            var tableType = new TableType { TableTypeId = request.TableTypeId, BarId = barId };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableTypeRepository.GetByID(request.TableTypeId))
                .Returns(tableType);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetByIdAsync(tableId))
                .ReturnsAsync(existingTable);

            _mockUnitOfWork.Setup(x => x.TableRepository.GetAsync(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(new List<Table> { duplicateTable, existingTable });

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataExistException>(
                () => _tableService.UpdateTable(tableId, request)
            );
        }

        [Fact]
        public async Task UpdateTableStatus_Success()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var newStatus = 1;

            var existingTable = new Table 
            { 
                TableId = tableId,
                Status = 0,
                IsDeleted = false,
                TableType = new TableType { BarId = barId }
            };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { existingTable }.AsQueryable());

            // Act
            await _tableService.UpdateTableStatus(tableId, newStatus);

            // Assert
            Assert.Equal(newStatus, existingTable.Status);
            _mockUnitOfWork.Verify(x => x.TableRepository.UpdateAsync(existingTable), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateTableStatus_TableNotFound_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var newStatus = 1;

            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table>().AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableService.UpdateTableStatus(tableId, newStatus)
            );
        }

        [Fact]
        public async Task UpdateTableStatus_TableDeleted_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var newStatus = 1;

            var existingTable = new Table 
            { 
                TableId = tableId,
                Status = 0,
                IsDeleted = true,
                TableType = new TableType { BarId = barId }
            };
            var account = new Account { AccountId = accountId, BarId = barId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { existingTable }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.DataNotFoundException>(
                () => _tableService.UpdateTableStatus(tableId, newStatus)
            );
        }

        [Fact]
        public async Task UpdateTableStatus_UnauthorizedAccess_ThrowsException()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var barId = Guid.NewGuid();
            var differentBarId = Guid.NewGuid();
            var newStatus = 1;

            var existingTable = new Table 
            { 
                TableId = tableId,
                Status = 0,
                IsDeleted = false,
                TableType = new TableType { BarId = barId }
            };
            var account = new Account { AccountId = accountId, BarId = differentBarId };

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.AccountRepository.GetByID(accountId))
                .Returns(account);

            _mockUnitOfWork.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Returns(new List<Table> { existingTable }.AsQueryable());

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.UnAuthorizedException>(
                () => _tableService.UpdateTableStatus(tableId, newStatus)
            );
        }

        [Fact]
        public async Task UpdateTableStatus_ThrowsException_WhenErrorOccurs()
        {
            // Arrange
            var tableId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var newStatus = 1;

            _mockAuthentication.Setup(x => x.GetUserIdFromHttpContext(It.IsAny<HttpContext>()))
                .Returns(accountId);

            _mockUnitOfWork.Setup(x => x.TableRepository.Get(
                It.IsAny<Expression<Func<Table, bool>>>(),
                It.IsAny<Func<IQueryable<Table>, IOrderedQueryable<Table>>>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>()))
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<CustomException.InternalServerErrorException>(
                () => _tableService.UpdateTableStatus(tableId, newStatus)
            );
        }
    }
}
