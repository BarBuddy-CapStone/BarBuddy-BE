using Application.DTOs.TableTypeDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface ITableTypeService
    {
        Task<List<TableTypeDtoResponse>> GetAll();
        Task<TableTypeDtoResponse?> GetById(string TableTypeId);
        Task CreateTableType(TableTypeDtoRequest request);
        Task<bool> UpdateTableType(TableTypeDtoRequest request, string TableTypeId);
        Task<int> DeleteTableType(string TableTypeId);
    }
}
