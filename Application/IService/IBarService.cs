using Application.DTOs.Bar;

namespace Application.IService
{
    public interface IBarService
    {
        Task<IEnumerable<BarResponse>> GetAllBar();
        Task<BarResponse> CreateBar(BarRequest request);
        Task<BarResponse> GetBarById(string barId);
        Task<BarResponse> UpdateBarById(string barId, BarRequest request);
    }
}
