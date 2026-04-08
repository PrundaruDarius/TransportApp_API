using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TransportApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public StationsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Data", "stations.json");

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "stations.json not found" });

            var json = System.IO.File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<object>(json);

            return Ok(data);
        }
    }
}