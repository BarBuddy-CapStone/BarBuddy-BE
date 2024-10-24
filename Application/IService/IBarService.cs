using Application.DTOs.Bar;
using Domain.Common;

namespace Application.IService
{
    public interface IBarService
    {
        Task<IEnumerable<BarResponse>> GetAllBar();
        Task<BarResponse> CreateBar(BarRequest request);
        Task<BarResponse> GetBarById(Guid barId);
        Task<BarResponse> UpdateBarById(Guid barId, BarRequest request);
        Task<IEnumerable<OnlyBarResponse>> GetAllBarWithFeedback(ObjectQuery query);
        Task<BarResponse> GetBarByIdWithFeedback(Guid barId);
        Task<BarResponse> GetBarByIdWithTable(Guid barId);
    }
}
