using Application.DTOs.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface ITableService
    {
        Task<(List<TableResponse> response, int TotalPage, string TableTypeName)> GetAll(Guid? BarId, Guid TableTypeId, int? Status, int PageIndex, int PageSize);
        Task CreateTable(CreateTableRequest request);
        Task UpdateTable(Guid TableId, UpdateTableRequest request);
        Task DeleteTable(Guid TableId);
    }
}
