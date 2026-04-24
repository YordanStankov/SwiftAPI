using SwiftAPI.Models;

namespace SwiftAPI.Data
{
    public interface IMT103Repository
    {
        Task InitializeAsync();
        Task<long> AddAsync(MT103 mt103);
        Task<List<MT103>> GetAllAsync();
        Task<MT103?> GetByIdAsync(long id);
    }
}
