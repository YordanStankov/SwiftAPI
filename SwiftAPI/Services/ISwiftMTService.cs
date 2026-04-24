using Microsoft.AspNetCore.Mvc;
using SwiftAPI.Models;

namespace SwiftAPI.Services
{
    public interface ISwiftMTService
    {
        Task<IActionResult> ReadMT103Async(IFormFile file);
        Task<MT103?> GetByIDAsync(long id);
        Task<List<MT103>> GetAllAsync();
    }
}
