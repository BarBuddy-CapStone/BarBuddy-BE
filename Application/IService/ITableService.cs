using Application.DTOs.Bar;
using Application.DTOs.BarTime;
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
        Task<(List<TableResponse> response, int TotalPage)> GetAll(Guid? TableTypeId, string? TableName, int? Status, int PageIndex, int PageSize);
        Task CreateTable(CreateTableRequest request);
        Task UpdateTable(Guid TableId, UpdateTableRequest request);
        Task UpdateTableStatus(Guid TableId, int status);
        Task<bool> DeleteTable(Guid TableId);
        Task<(List<TableResponse> response, int TotalPage, List<BarTimeResponse> barTimes, double timeSlot)> GetAllOfBar(Guid BarId, Guid? TableTypeId,
            string? TableName, int? Status, int PageIndex, int PageSize, DateTime BookingDate, TimeSpan BookingTime);
    }
}
