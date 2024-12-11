using Application.Common;
using Application.DTOs.Bar;
using Application.DTOs.BarTime;
using Application.DTOs.Table;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Azure.Core;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using Domain.Utils;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Domain.CustomException.CustomException;

namespace Application.Service
{
    public class TableService : ITableService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthentication _authentication;
        private readonly IMapper _mapper;
        public TableService(IUnitOfWork unitOfWork, IAuthentication authentication,
                            IHttpContextAccessor contextAccessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _authentication = authentication;
            _contextAccessor = contextAccessor;
            _mapper = mapper;
        }

        public async Task CreateTable(CreateTableRequest request)
        {
            try
            {
                var isExistTT = _unitOfWork.TableTypeRepository.Get(x => x.TableTypeId.Equals(request.TableTypeId) && x.IsDeleted == false).FirstOrDefault();
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                  x.Status == (int)PrefixValueEnum.Active)
                                .FirstOrDefault();

                if (isExistTT == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy loại bàn !");
                }

                var existedName = (await _unitOfWork.TableRepository.GetAsync(filter: t => t.TableName == request.TableName && t.TableTypeId == request.TableTypeId)).FirstOrDefault();
                if (existedName != null)
                {
                    throw new CustomException.DataExistException("Trùng tên bàn");
                }
                var getBar = _unitOfWork.BarRepository.Get(filter: x => x.BarId.Equals(isExistTT.BarId) &&
                                                   x.Status == PrefixKeyConstant.TRUE)
                                      .FirstOrDefault();
                if (getBar == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy quán Bar");
                }

                if (getAccount.BarId != isExistTT.BarId)
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                var Table = new Table
                {
                    //BarId = request.BarId,
                    TableTypeId = request.TableTypeId,
                    TableName = request.TableName,
                    Status = request.Status,
                    IsDeleted = false
                };
                await _unitOfWork.TableRepository.InsertAsync(Table);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.InternalServerErrorException ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<bool> DeleteTable(Guid TableId)
        {
            try
            {
                var existedTable = await _unitOfWork.TableRepository.GetByIdAsync(TableId);
                if (existedTable == null)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }
                // Check future booking
                var bookingTable = await _unitOfWork.BookingTableRepository.GetAsync(
                    filter: bt => bt.TableId == TableId && bt.Booking.BookingDate >= DateTimeOffset.Now,
                    includeProperties: "Booking");

                if (bookingTable.Any())
                {
                    return false;
                }

                existedTable.IsDeleted = true;
                await _unitOfWork.TableRepository.UpdateAsync(existedTable);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<TableResponse> response, int TotalPage)> GetAll(Guid? TableTypeId, string? TableName, int? Status, int PageIndex, int PageSize)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                            .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                              x.Status == (int)PrefixValueEnum.Active,
                                                 includeProperties: "Role")
                                            .FirstOrDefault();

                var getBar = _unitOfWork.BarRepository.GetByID(getAccount.BarId);
                if (getBar.Status == PrefixKeyConstant.FALSE && getAccount.Role.RoleName.Equals(PrefixKeyConstant.STAFF))
                {
                    throw new CustomException.UnAuthorizedException("Hiện tại bạn không thể truy cập vào quán bar này được !");
                }

                var responses = new List<TableResponse>();
                int totalPage = 1;

                // filter expression
                Expression<Func<Table, bool>> filter = t =>
                (Status == null || t.Status == Status) &&
                //(BarId == null || t.BarId == BarId) &&
                (TableTypeId == null || t.TableTypeId == TableTypeId) &&
                (TableName == null || t.TableName.Contains(TableName)) &&
                t.IsDeleted == false && t.TableType.IsDeleted == false;

                var totalTable = (await _unitOfWork.TableRepository.GetAsync(filter: filter, includeProperties: "TableType")).Count();

                if (totalTable > 0)
                {
                    if (totalTable > PageSize)
                    {
                        if (PageSize == 1)
                        {
                            totalPage = (totalTable / PageSize);
                        }
                        else
                        {
                            totalPage = (totalTable / PageSize) + 1;
                        }
                    }

                    var tablesWithPagination = await _unitOfWork.TableRepository.GetAsync(filter: filter, pageIndex: PageIndex, pageSize: PageSize, includeProperties: "TableType");

                    foreach (var table in tablesWithPagination)
                    {
                        var tableResponse = new TableResponse
                        {
                            //BarId = table.BarId,
                            TableTypeId = table.TableTypeId,
                            MinimumGuest = table.TableType.MinimumGuest,
                            MaximumGuest = table.TableType.MaximumGuest,
                            MinimumPrice = table.TableType.MinimumPrice,
                            Status = table.Status,
                            TableName = table.TableName,
                            TableTypeName = table.TableType.TypeName,
                            TableId = table.TableId
                        };
                        responses.Add(tableResponse);
                    }
                }

                return (responses, totalPage);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<TableResponse> response, int TotalPage, List<BarTimeResponse> barTimes, double timeSlot)> GetAllOfBar(Guid BarId, Guid? TableTypeId, 
            string? TableName, int? Status, int PageIndex, int PageSize, DateTime RequestDate, TimeSpan RequestTime)
        {
            try
            {
                var responses = new List<TableResponse>();
                int totalPage = 1;

                var bar = (await _unitOfWork.BarRepository.GetAsync(filter: x => x.BarId.Equals(BarId) 
                                                                         && x.Status == PrefixKeyConstant.TRUE,
                        includeProperties: "BarTimes")).FirstOrDefault()
                        ?? throw new CustomException.DataNotFoundException("Không tìm thấy quán bar");
                var timeSlot = bar.TimeSlot;
                var barTimeResponses = _mapper.Map<List<BarTimeResponse>>(bar.BarTimes);

                // filter expression
                Expression<Func<Table, bool>> filter = t =>
                (Status == null || t.Status == Status) &&
                (t.TableType.Bar.BarId == BarId) &&
                (TableTypeId == null || t.TableTypeId == TableTypeId) &&
                (TableName == null || t.TableName.Contains(TableName)) &&
                t.IsDeleted == false && t.TableType.IsDeleted == false;

                if (RequestDate.Date < DateTime.Now.Date) 
                    throw new CustomException.InvalidDataException("Ngày không hợp lệ");

                var totalTable = (await _unitOfWork.TableRepository.GetAsync(filter: filter, includeProperties: "TableType")).Count();

                if (totalTable > 0)
                {
                    if (totalTable > PageSize)
                    {
                        if (PageSize == 1)
                        {
                            totalPage = (totalTable / PageSize);
                        }
                        else
                        {
                            totalPage = (totalTable / PageSize) + 1;
                        }
                    }

                    var tablesWithPagination = await _unitOfWork.TableRepository.GetAsync(
                        filter: filter,
                        orderBy: q => q.OrderBy(x => x.TableName),
                        pageIndex: PageIndex,
                        pageSize: PageSize,
                        includeProperties: "TableType.Bar.BarTimes");

                    bool isValidTime = bar.BarTimes.Where(barTime => barTime.DayOfWeek.Equals((int)RequestDate.DayOfWeek))
                        .Any(barTime =>
                    {
                        if (barTime.StartTime < barTime.EndTime)
                        {
                            return Utils.IsValidSlot(RequestTime, barTime.StartTime, barTime.EndTime, bar.TimeSlot);
                        }
                        else
                        {
                            return Utils.IsValidSlot(RequestTime, barTime.StartTime, TimeSpan.FromHours(24), bar.TimeSlot) ||
                                   Utils.IsValidSlot(RequestTime, TimeSpan.Zero, barTime.EndTime, bar.TimeSlot);
                        }
                    });
                    foreach (var table in tablesWithPagination)
                    {
                        int currentStatus = 3;
                        if (isValidTime)
                        {
                            currentStatus = await GetTableStatus(table, RequestDate, RequestTime);
                        }
                        
                        var tableResponse = new TableResponse
                        {
                            TableTypeId = table.TableTypeId,
                            MinimumGuest = table.TableType.MinimumGuest,
                            MaximumGuest = table.TableType.MaximumGuest,
                            MinimumPrice = table.TableType.MinimumPrice,
                            Status = currentStatus,
                            TableName = table.TableName,
                            TableTypeName = table.TableType.TypeName,
                            TableId = table.TableId
                        };
                        responses.Add(tableResponse);
                    }
                }

                return (responses, totalPage, barTimeResponses, timeSlot);
            }
            catch (CustomException.InvalidDataException ex)
            {
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        private async Task<int> GetTableStatus(Table table, DateTime bookingDate, TimeSpan bookingTime)
        {
            try
            {
                // Lấy tất cả booking table của bàn trong ngày được chọn
                var bookingTables = await _unitOfWork.BookingTableRepository.GetAsync(
                    filter: bt => bt.TableId == table.TableId &&
                                 bt.Booking.BookingDate.Date == bookingDate.Date &&
                                 bt.Booking.BookingTime.Hours == bookingTime.Hours &&
                                 bt.Booking.Status != (int)PrefixValueEnum.Cancelled,
                    includeProperties: "Booking");

                if (!bookingTables.Any())
                    return (int)TableStatusEnum.Available;

                foreach (var bt in bookingTables)
                {
                    var booking = bt.Booking;
                    
                    // Thời gian bắt đầu và kết thúc của booking hiện tại đang xét
                    var bookingStartTime = booking.BookingTime;
                    var bookingEndTime = booking.BookingTime.Add(TimeSpan.FromHours(3)); // Giả sử mỗi booking kéo dài 3 tiếng
                    
                    // Thời gian của booking mới muốn đặt
                    var requestedStartTime = bookingTime;
                    var requestedEndTime = bookingTime.Add(TimeSpan.FromHours(3));

                    // Kiểm tra xem có bị trùng thời gian không
                    bool isTimeOverlap = !(requestedEndTime <= bookingStartTime || requestedStartTime >= bookingEndTime);

                    if (isTimeOverlap)
                    {
                        // Nếu booking hiện tại đang trong trạng thái check-in
                        if (booking.Status == (int)PrefixValueEnum.Serving)
                            return (int)TableStatusEnum.InUse;
                        
                        // Nếu booking hiện tại đang trong trạng thái pending
                        if (booking.Status == (int)PrefixValueEnum.PendingBooking)
                            return (int)TableStatusEnum.Reserved;
                    }
                }

                // Nếu không có booking nào trùng thời gian
                return (int)TableStatusEnum.Available;
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task UpdateTable(Guid TableId, UpdateTableRequest request)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                  x.Status == (int)PrefixValueEnum.Active)
                                .FirstOrDefault();
                var isExistTT = _unitOfWork.TableTypeRepository
                                           .Get(x => x.TableTypeId.Equals(request.TableTypeId) && 
                                                     x.BarId.Equals(getAccount.BarId) && 
                                                     x.Bar.Status == PrefixKeyConstant.TRUE, 
                                                     includeProperties: "Bar")
                                           .FirstOrDefault();

                if (isExistTT == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy loại bàn !");
                }

                var existedTable = await _unitOfWork.TableRepository.GetByIdAsync(TableId);
                if (existedTable == null)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }

                if(existedTable.TableTypeId != isExistTT.TableTypeId)
                {
                    throw new CustomException.InvalidDataException("Loại bàn không hợp lệ với bàn muốn cập nhật !");
                }

                if(!isExistTT.BarId.Equals(getAccount.BarId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (existedTable.IsDeleted)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }
                var existedName = (await _unitOfWork.TableRepository.GetAsync(t => t.TableName == request.TableName)).Count();
                if (existedTable.TableName.Equals(request.TableName))
                {
                    if (existedName > 1)
                    {
                        throw new CustomException.DataExistException("Trùng tên bàn đã có trong hệ thống");
                    }
                }
                else
                {
                    if (existedName > 0)
                    {
                        throw new CustomException.DataExistException("Trùng tên bàn đã có trong hệ thống");
                    }
                }

                existedTable.TableName = request.TableName;
                existedTable.Status = request.Status;
                await _unitOfWork.TableRepository.UpdateAsync(existedTable);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.InvalidDataException ex)
            {
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                throw new CustomException.UnAuthorizedException(ex.Message);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (CustomException.DataExistException ex)
            {
                throw new CustomException.DataExistException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task UpdateTableStatus(Guid TableId, int status)
        {
            try
            {

                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                  x.Status == (int)PrefixValueEnum.Active)
                                .FirstOrDefault();

                var existedTable = _unitOfWork.TableRepository
                                            .Get(filter: x => x.TableId.Equals(TableId) && 
                                                              x.TableType.Bar.Status == PrefixKeyConstant.TRUE,
                                                includeProperties: "TableType.Bar").FirstOrDefault();
                if (existedTable == null)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }

                if (!existedTable.TableType.BarId.Equals(getAccount.BarId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (existedTable.IsDeleted)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }
                existedTable.Status = status;
                await _unitOfWork.TableRepository.UpdateAsync(existedTable);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                throw new CustomException.UnAuthorizedException(ex.Message);
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<TableInfoResponse>> GetTableInformationByCustomer(List<Guid> TableIdList)
        {
            try
            {
                var TableListResponse = new List<TableInfoResponse>();
                foreach (var tableId in TableIdList)
                {
                    var table = (await _unitOfWork.TableRepository.GetAsync(t => t.TableId == tableId, includeProperties: "TableType")).FirstOrDefault();
                    if(table != null)
                    {
                        var tableResponse = new TableInfoResponse
                        {
                            TableId = table.TableId,
                            MaximumGuest = table.TableType.MaximumGuest,
                            MinimumGuest = table.TableType.MinimumGuest,
                            MinimumPrice = table.TableType.MinimumPrice,
                            TableName = table.TableName,
                            TableTypeId = table.TableTypeId,
                            TableTypeName = table.TableName,
                        };

                        TableListResponse.Add(tableResponse);
                    }
                }
                return TableListResponse;
            }
            catch (CustomException.DataNotFoundException ex)
            {
                throw new CustomException.DataNotFoundException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
