using Application.DTOs.TableType;
using Application.Interfaces;
using Application.IService;
using AutoMapper;
using Azure.Core;
using Domain.Common;
using Domain.Constants;
using Domain.CustomException;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Service
{
    public class TableTypeService : ITableTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAuthentication _authentication;

        public TableTypeService(IUnitOfWork unitOfWork, IMapper mapper,
                                IAuthentication authentication, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _authentication = authentication;
            _contextAccessor = contextAccessor;
        }

        public async Task CreateTableType(TableTypeRequest request)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                  x.Status == (int)PrefixValueEnum.Active)
                                .FirstOrDefault();
                var getBar = _unitOfWork.BarRepository.Get(filter: x => x.BarId.Equals(request.BarId))
                                                      .FirstOrDefault();
                if (getBar == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy quán Bar");
                }

                if (getAccount.BarId != request.BarId)
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (request.MaximumGuest < request.MinimumGuest)
                {
                    throw new CustomException.InvalidDataException("Số lượng đa tối thiểu phải bé hơn số lượng khách tối đa");
                }
                var tableType = new TableType
                {
                    TableTypeId = Guid.NewGuid(),
                    BarId = request.BarId,
                    Description = request.Description,
                    MaximumGuest = request.MaximumGuest,
                    MinimumGuest = request.MinimumGuest,
                    MinimumPrice = request.MinimumPrice,
                    TypeName = request.TypeName,
                    IsDeleted = false
                };
                await _unitOfWork.TableTypeRepository.InsertAsync(tableType);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                throw new CustomException.UnAuthorizedException(ex.Message);
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

        public async Task<bool> DeleteTableType(Guid TableTypeId)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                  x.Status == (int)PrefixValueEnum.Active)
                                .FirstOrDefault();

                var existedTableType = (await _unitOfWork.TableTypeRepository
                                                            .GetAsync(filter: t => t.TableTypeId == TableTypeId &&
                                                            t.IsDeleted == PrefixKeyConstant.FALSE))
                                                            .FirstOrDefault();
                if (existedTableType == null)
                {
                    throw new CustomException.DataNotFoundException("Loại bàn không tồn tại");
                }
                var getBar = _unitOfWork.BarRepository.Get(filter: x => x.BarId.Equals(existedTableType.BarId) &&
                                                                   x.Status == PrefixKeyConstant.TRUE)
                                                      .FirstOrDefault();
                if (getBar == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy quán Bar");
                }

                if (getAccount.BarId != existedTableType.BarId)
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                //var tables = await _unitOfWork.TableRepository.GetAsync(t => t.TableTypeId == TableTypeId && t.IsDeleted == false);
                //if (tables.Any())
                //{
                //    return false;
                //}
                existedTableType.IsDeleted = true;
                await _unitOfWork.TableTypeRepository.UpdateAsync(existedTableType);
                await _unitOfWork.SaveAsync();
                return true;
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

        public async Task<List<TableTypeResponse>> GetAll()
        {
            try
            {
                List<TableTypeResponse> responses = new List<TableTypeResponse>();

                var tableTypes = (await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.IsDeleted == false, orderBy: o => o.OrderByDescending(t => t.MinimumPrice))).ToList();

                foreach (var tableType in tableTypes)
                {
                    var tableTypeDto = _mapper.Map<TableTypeResponse>(tableType);
                    responses.Add(tableTypeDto);
                }

                return responses;
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<TableTypeResponse>> GetAllForAdmin(int Status)
        {
            try
            {
                List<TableTypeResponse> responses = new List<TableTypeResponse>();
                List<TableType> tableTypes = new List<TableType>();
                int TotalPage = 1;

                switch (Status)
                {
                    case 0:
                        tableTypes = (await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.IsDeleted == false, orderBy: o => o.OrderByDescending(t => t.MinimumPrice))).ToList();
                        break;
                    case 1:
                        var tableTypesTemp1 = await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.IsDeleted == false);
                        foreach (var tableTypeTemp in tableTypesTemp1)
                        {
                            var isExistingTable = (await _unitOfWork.TableRepository.GetAsync(filter: t => t.TableTypeId == tableTypeTemp.TableTypeId && t.IsDeleted == false)).Any();
                            if (isExistingTable)
                            {
                                tableTypes.Add(tableTypeTemp);
                            }
                        }
                        break;
                    case 2:
                        var tableTypesTemp2 = await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.IsDeleted == false);
                        foreach (var tableTypeTemp in tableTypesTemp2)
                        {
                            var numOfTables = (await _unitOfWork.TableRepository.GetAsync(filter: t => t.TableTypeId == tableTypeTemp.TableTypeId)).Count();
                            var numOfDeletedTables = (await _unitOfWork.TableRepository.GetAsync(filter: t => t.TableTypeId == tableTypeTemp.TableTypeId && t.IsDeleted == true)).Count();
                            int subtraction = numOfTables - numOfDeletedTables;
                            if (subtraction == 0)
                            {
                                tableTypes.Add(tableTypeTemp);
                            }
                        }
                        break;
                    default:
                        throw new CustomException.InvalidDataException("Trạng thái không tồn tại");
                }

                foreach (var tableType in tableTypes)
                {
                    var tableTypeDto = _mapper.Map<TableTypeResponse>(tableType);
                    responses.Add(tableTypeDto);
                }

                return responses;
            }
            catch (CustomException.InvalidDataException ex)
            {
                throw new CustomException.InvalidDataException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<TableTypeResponse?> GetById(Guid TableTypeId)
        {
            try
            {
                TableTypeResponse? tableTypeDto = null;

                var tableType = (await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.TableTypeId == TableTypeId && t.IsDeleted == false)).FirstOrDefault();

                if (tableType != null)
                {
                    tableTypeDto = _mapper.Map<TableTypeResponse>(tableType);
                }
                else
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy loại bàn");
                }
                return tableTypeDto;
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

        public async Task UpdateTableType(TableTypeRequest request, Guid TableTypeId)
        {
            try
            {
                var accountId = _authentication.GetUserIdFromHttpContext(_contextAccessor.HttpContext);
                var getAccount = _unitOfWork.AccountRepository
                                .Get(filter: x => x.AccountId.Equals(accountId) &&
                                                  x.Status == (int)PrefixValueEnum.Active)
                                .FirstOrDefault();
                var getBar = _unitOfWork.BarRepository.Get(filter: x => x.BarId.Equals(request.BarId))
                                                      .FirstOrDefault();
                var existedTableType = await _unitOfWork.TableTypeRepository.GetByIdAsync(TableTypeId);

                if (existedTableType == null)
                {
                    throw new CustomException.DataNotFoundException("Loại bàn không tồn tại");
                }

                if (getBar == null)
                {
                    throw new CustomException.DataNotFoundException("Không tìm thấy quán Bar");
                }

                if (getAccount.BarId != request.BarId || getAccount.BarId != existedTableType.BarId)
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (request.BarId != existedTableType.BarId)
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }

                if (request.MaximumGuest < request.MinimumGuest)
                {
                    throw new CustomException.InvalidDataException("Số lượng đa tối thiểu phải bé hơn số lượng khách tối đa");
                }

                existedTableType.MinimumPrice = request.MinimumPrice;
                existedTableType.MinimumGuest = request.MinimumGuest;
                existedTableType.MaximumGuest = request.MaximumGuest;
                existedTableType.Description = request.Description;
                existedTableType.TypeName = request.TypeName;
                await _unitOfWork.TableTypeRepository.UpdateAsync(existedTableType);
                await _unitOfWork.SaveAsync();
            }
            catch (CustomException.UnAuthorizedException ex)
            {
                throw new CustomException.UnAuthorizedException(ex.Message);
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

        public async Task<PaginationTableTypeResponse> GetAllTTOfBar(Guid barId, ObjectQuery query)
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
                if (getAccount.BarId.HasValue && getBar.Status == PrefixKeyConstant.FALSE && !getAccount.Role.RoleName.Equals(PrefixKeyConstant.MANAGER))
                {
                    throw new CustomException.UnAuthorizedException("Hiện tại bạn không thể truy cập vào quán bar này được !");
                }

                if (getAccount.BarId.HasValue && !getAccount.BarId.Equals(barId))
                {
                    throw new CustomException.UnAuthorizedException("Bạn không có quyền truy cập vào quán bar này !");
                }


                var pageIndex = query.PageIndex ?? 1;
                var pageSize = query.PageSize ?? 6;

                var tableTypes = (await _unitOfWork.TableTypeRepository
                                                        .GetAsync(filter: t => t.IsDeleted == false
                                                                            && t.BarId.Equals(barId),
                                                        orderBy: o => o.OrderByDescending(t => t.MinimumPrice)))
                                                        .ToList();

                var totalItems = tableTypes.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var dataResponse = _mapper.Map<List<TableTypeResponse>>(tableTypes.Skip((pageIndex - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToList());

                return new PaginationTableTypeResponse
                {
                    TableTypeResponses = dataResponse,
                    TotalPages = totalPages,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };
            }
            catch (Exception ex)
            {
                throw new CustomException.InternalServerErrorException(ex.Message);
            }
        }

        public async Task<List<TableTypeResponse>> GetAllTTOfBarByCustomer(Guid barId)
        {
            try
            {
                var existingBar = _unitOfWork.BarRepository
                                             .Get(filter: x => x.BarId.Equals(barId) && 
                                                               x.Status == PrefixKeyConstant.TRUE)
                                             .FirstOrDefault();

                if (existingBar == null)
                {
                    throw new CustomException.DataNotFoundException("Bar không tồn tại");
                }

                var tableTypes = (await _unitOfWork.TableTypeRepository
                                                        .GetAsync(filter: t => t.IsDeleted == false
                                                                            && t.BarId.Equals(barId),
                                                        orderBy: o => o.OrderByDescending(t => t.MinimumPrice)))
                                                        .ToList();

                List<TableTypeResponse> response = new List<TableTypeResponse>();
                foreach (var tableType in tableTypes)
                {
                    response.Add(_mapper.Map<TableTypeResponse>(tableType));
                }

                return response;
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
