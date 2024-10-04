using Application.DTOs.Bar;

namespace Application.IService
{
    public interface IBarService
    {
        Task<IEnumerable<BarResponse>> GetAllBar();
        Task<BarResponse> CreateBar(BarRequest request);
        Task<BarResponse> GetBarById(Guid barId);
        Task<BarResponse> UpdateBarById(Guid barId, BarRequest request);
        Task<IEnumerable<BarResponse>> GetAllBarWithFeedback();
        Task<BarResponse> GetBarByIdWithFeedback(Guid barId);
    }
}
