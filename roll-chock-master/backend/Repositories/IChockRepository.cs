using RollChockBackend.Models;

namespace RollChockBackend.Repositories
{
    public interface IChockRepository
    {
        Task<ChockLookupsDto> GetLookupsAsync();
        Task<ChockQueryResponse> QueryChockAsync(string chockId, string chockType);
        Task<(bool success, bool wasUpdate)> SaveChockAsync(ChockSaveRequest input);
    }
}
