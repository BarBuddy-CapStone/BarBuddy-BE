using Application.DTOs.TableTypeDto;
using Application.IService;
using AutoMapper;
using Azure.Core;
using Domain.Entities;
using Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class TableTypeService : ITableTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TableTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateTableType(TableTypeDtoRequest request)
        {
            try
            {
                var tableType = new TableType
                {
                    TableTypeId = Guid.NewGuid().ToString(),
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
            catch (Exception ex) { 
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> DeleteTableType(string TableTypeId)
        {
            try
            {
                var existedTableType = (await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.TableTypeId == TableTypeId && t.IsDeleted == false)).FirstOrDefault();
                if (existedTableType == null)
                {
                    return 1;
                }
                var tables = await _unitOfWork.TableRepository.GetAsync(t => t.TableTypeId == TableTypeId && t.IsDeleted == false);
                if (tables.Any())
                {
                    return 2;
                }
                existedTableType.IsDeleted = true;
                await _unitOfWork.TableTypeRepository.UpdateAsync(existedTableType);
                await _unitOfWork.SaveAsync();
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<TableTypeDtoResponse>> GetAll()
        {
            try
            {
                List<TableTypeDtoResponse> responses = new List<TableTypeDtoResponse>();

                var tableTypes = (await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.IsDeleted == false, orderBy: o => o.OrderByDescending(t => t.MinimumPrice))).ToList();

                foreach ( var tableType in tableTypes)
                {
                    var tableTypeDto = _mapper.Map<TableTypeDtoResponse>(tableType);
                    responses.Add(tableTypeDto);
                }

                return responses;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TableTypeDtoResponse?> GetById(string TableTypeId)
        {
            try
            {
                TableTypeDtoResponse? tableTypeDto = null;

                var tableType = (await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.TableTypeId == TableTypeId && t.IsDeleted == false)).FirstOrDefault();

                if(tableType != null)
                {
                    tableTypeDto = _mapper.Map<TableTypeDtoResponse>(tableType);
                }
                return tableTypeDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateTableType(TableTypeDtoRequest request, string TableTypeId)
        {
            try
            {
                var existedTableType = await _unitOfWork.TableTypeRepository.GetByIdAsync(TableTypeId);
                if (existedTableType == null)
                {
                    return false;
                }
                existedTableType.MinimumPrice = request.MinimumPrice;
                existedTableType.MinimumGuest = request.MinimumGuest;
                existedTableType.MaximumGuest = request.MaximumGuest;
                existedTableType.Description = request.Description;
                existedTableType.TypeName = request.TypeName;
                await _unitOfWork.TableTypeRepository.UpdateAsync(existedTableType);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
