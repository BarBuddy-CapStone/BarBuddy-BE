using Application.DTOs.TableType;
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

        public async Task CreateTableType(TableTypeRequest request)
        {
            try
            {
                var tableType = new TableType
                {
                    TableTypeId = Guid.NewGuid(),
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

        public async Task<int> DeleteTableType(Guid TableTypeId)
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

        public async Task<List<TableTypeResponse>> GetAll()
        {
            try
            {
                List<TableTypeResponse> responses = new List<TableTypeResponse>();

                var tableTypes = (await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.IsDeleted == false, orderBy: o => o.OrderByDescending(t => t.MinimumPrice))).ToList();

                foreach ( var tableType in tableTypes)
                {
                    var tableTypeDto = _mapper.Map<TableTypeResponse>(tableType);
                    responses.Add(tableTypeDto);
                }

                return responses;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TableTypeResponse?> GetById(Guid TableTypeId)
        {
            try
            {
                TableTypeResponse? tableTypeDto = null;

                var tableType = (await _unitOfWork.TableTypeRepository.GetAsync(filter: t => t.TableTypeId == TableTypeId && t.IsDeleted == false)).FirstOrDefault();

                if(tableType != null)
                {
                    tableTypeDto = _mapper.Map<TableTypeResponse>(tableType);
                }
                return tableTypeDto;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> UpdateTableType(TableTypeRequest request, Guid TableTypeId)
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
