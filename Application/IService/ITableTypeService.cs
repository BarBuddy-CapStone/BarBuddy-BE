using Application.DTOs.TableType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface ITableTypeService
    {
        Task<List<TableTypeResponse>> GetAll();
        Task<TableTypeResponse?> GetById(Guid TableTypeId);
        Task CreateTableType(TableTypeRequest request);
        Task UpdateTableType(TableTypeRequest request, Guid TableTypeId);
        Task<bool> DeleteTableType(Guid TableTypeId);
    }
}
