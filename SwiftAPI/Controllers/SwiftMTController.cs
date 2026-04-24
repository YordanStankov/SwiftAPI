using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SwiftAPI.Services;
using System.Runtime.CompilerServices;
namespace SwiftAPI.Controllers
{
    public class SwiftMTController : Controller
    {
        private readonly ISwiftMTService _swiftMTService;
        public SwiftMTController(ISwiftMTService swiftMTService)
        {

            _swiftMTService = swiftMTService;
        }
        [HttpPost("mt103/upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMT103(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");
            else if (Path.GetExtension(file.FileName).ToLower() != ".txt"
                && Path.GetExtension(file.FileName).ToLower() != ".mt")
                return BadRequest("Invalid file type");
            else
            {
                var result = await _swiftMTService.ReadMT103Async(file);
                return Ok("Transaction processed successfully");
            }
        }
        [HttpGet("mt103/{id}")]
        public async Task<IActionResult> GetMT103ById(long id)
        {
            var result = await _swiftMTService.GetByIDAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        [HttpGet("allMt103")]
        public async Task<IActionResult> GetAllMT103()
        {
            var result = await _swiftMTService.GetAllAsync();
            return Ok(result);
        }
    }
}