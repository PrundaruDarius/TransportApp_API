using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TransportApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public LinesController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Data", "lines.json");

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "lines.json not found" });

            var json = System.IO.File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<object>(json);

            return Ok(data);
        }
    }
}