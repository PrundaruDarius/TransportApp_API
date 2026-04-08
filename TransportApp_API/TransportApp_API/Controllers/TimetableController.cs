using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TransportApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimetableController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public TimetableController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var filePath = Path.Combine(_environment.ContentRootPath, "Data", "timetable.json");

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "timetable.json not found" });

            var json = System.IO.File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<object>(json);

            return Ok(data);
        }
    }
}