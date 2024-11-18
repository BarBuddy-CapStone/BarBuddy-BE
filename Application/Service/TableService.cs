﻿using Application.DTOs.Table;
using Application.IService;
using Azure.Core;
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
                var existedName = (await _unitOfWork.TableRepository.GetAsync(filter: t => t.TableName == request.TableName && t.TableTypeId == request.TableTypeId)).FirstOrDefault();
                if(existedName != null)
                {
                    throw new CustomException.DataExistException("Trùng tên bàn");
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
            catch (CustomException.InternalServerErrorException ex) {
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
            catch (Exception ex) {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<TableResponse> response, int TotalPage)> GetAll(Guid? TableTypeId, string? TableName, int? Status, int PageIndex, int PageSize)
        {
            try
            {
                var responses = new List<TableResponse>();
                int totalPage = 1;

                // filter expression
                Expression<Func<Table, bool>> filter = t =>
                (Status == null || t.Status == Status) &&
                //(BarId == null || t.BarId == BarId) &&
                (TableTypeId == null || t.TableTypeId == TableTypeId) &&
                (TableName == null || t.TableName.Contains(TableName)) &&
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

                return (responses, totalPage);
            } catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<(List<TableResponse> response, int TotalPage)> GetAllOfBar(Guid BarId, Guid? TableTypeId, string? TableName, int? Status, int PageIndex, int PageSize)
        {
            try
            {
                var responses = new List<TableResponse>();
                int totalPage = 1;

                // filter expression
                Expression<Func<Table, bool>> filter = t =>
                (Status == null || t.Status == Status) &&
                (t.TableType.Bar.BarId == BarId) &&
                (TableTypeId == null || t.TableTypeId == TableTypeId) &&
                (TableName == null || t.TableName.Contains(TableName)) &&
                t.IsDeleted == false;

                var totalTable = (await _unitOfWork.TableRepository.GetAsync(filter: filter)).Count();

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

                    var tablesWithPagination = await _unitOfWork.TableRepository.GetAsync(filter: filter, 
                            orderBy: q => q.OrderBy(x => x.TableName),
                            pageIndex: PageIndex, 
                            pageSize: PageSize, 
                            includeProperties: "TableType.Bar");

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

        public async Task UpdateTable(Guid TableId, UpdateTableRequest request)
        {
            try
            {
                var existedTable = await _unitOfWork.TableRepository.GetByIdAsync(TableId);
                if(existedTable == null)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }
                if (existedTable.IsDeleted)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }
                var existedName = (await _unitOfWork.TableRepository.GetAsync(t => t.TableName == request.TableName)).Count();
                if (existedTable.TableName.Equals(request.TableName))
                {
                    if(existedName > 1)
                    {
                        throw new CustomException.DataExistException("Trùng tên bàn đã có trong hệ thống");
                    }
                }
                else
                {
                    if (existedName > 0) { 
                        throw new CustomException.DataExistException("Trùng tên bàn đã có trong hệ thống");
                    }
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

        public async Task UpdateTableStatus(Guid TableId, int status)
        {
            try
            {
                var existedTable = await _unitOfWork.TableRepository.GetByIdAsync(TableId);
                if (existedTable == null)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }
                if (existedTable.IsDeleted)
                {
                    throw new CustomException.DataNotFoundException("Table Id không tồn tại");
                }
                existedTable.Status = status;
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
