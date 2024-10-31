using Application.DTOs.Bar;
using Domain.Common;

namespace Application.IService
{
    public interface IBarService
    {
        Task<IEnumerable<BarResponse>> GetAllBar(ObjectQuery query);
        Task CreateBar(CreateBarRequest request);
        Task<BarResponse> GetBarById(Guid barId);
        Task<OnlyBarResponse> UpdateBarById(UpdateBarRequest request);
        Task<IEnumerable<OnlyBarResponse>> GetAllBarWithFeedback(ObjectQuery query);
        Task<BarResponse> GetBarByIdWithFeedback(Guid barId);
        Task<BarResponse> GetBarByIdWithTable(Guid barId);
        Task<IEnumerable<OnlyBarResponse>> GetAllAvailableBars(DateTime dateTime);
    }
}
