using Application.DTOs.Table;
using Application.IService;
using Domain.CustomException;
using Domain.Entities;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class TableService : ITableService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TableService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateTable(CreateTableRequest request)
        {
            try
            {
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
            catch (Exception ex) {
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
                var bookingTable = await _unitOfWork.BookingTableRepository.GetAsync(filter: bt => bt.TableId == TableId && bt.ReservationDate >= DateTimeOffset.Now);
                if (bookingTable.Any())
                {
                    return false;
                }

                existedTable.IsDeleted = true;
                await _unitOfWork.TableRepository.UpdateAsync(existedTable);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex) {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<TableResponse> response, int TotalPage, string TableTypeName, Guid TableTypeId)> GetAll(Guid? BarId, Guid TableTypeId, int? Status, int PageIndex, int PageSize)
        {
            try
            {
                var responses = new List<TableResponse>();
                int totalPage = 1;

                var tableType = await _unitOfWork.TableTypeRepository.GetByIdAsync(TableTypeId);

                if (tableType == null) {
                    throw new Exception("Table Type not found");
                }

                string TableTypeName = tableType.TypeName;
                Guid tableTypeId = tableType.TableTypeId;

                // filter expression
                Expression<Func<Table, bool>> filter = t =>
                (Status == null || t.Status == Status) &&
                //(BarId == null || t.BarId == BarId) &&
                t.TableTypeId == TableTypeId &&
                t.IsDeleted == false;
                
                var totalTable = (await _unitOfWork.TableRepository.GetAsync(filter: filter)).Count();

                if(totalTable > 0)
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

                return (responses, totalPage, TableTypeName, tableTypeId);
            } catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task UpdateTable(Guid TableId, UpdateTableRequest request)
        {
            try
            {
                var existedTable = await _unitOfWork.TableRepository.GetByIdAsync(TableId);
                if(existedTable == null)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }

                existedTable.TableName = request.TableName;
                existedTable.Status = request.Status;
                await _unitOfWork.TableRepository.UpdateAsync(existedTable);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }
    }
}
