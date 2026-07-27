using RollChockBackend.Models;

namespace RollChockBackend.Repositories
{
    public interface IChockRepository
    {
        Task<ChockLookupsDto> GetLookupsAsync();
        Task<ChockQueryResponse> QueryChockAsync(string chockId, string chockType);
        Task<ChockTypeConfigDto> GetTypeConfigAsync(string chockType, string? chockId);
        Task<(bool success, bool wasUpdate)> SaveChockAsync(ChockSaveRequest input);
    }
}
