using Microsoft.AspNetCore.Mvc;

namespace TransportApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public DocumentsController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var folderPath = Path.Combine(_environment.ContentRootPath, "Documents");

            if (!Directory.Exists(folderPath))
                return NotFound(new { message = "Documents folder not found" });

            var files = Directory.GetFiles(folderPath, "*.pdf")
                .Select(Path.GetFileName)
                .ToList();

            return Ok(files);
        }

        [HttpGet("{fileName}")]
        public IActionResult GetFile(string fileName)
        {
            var folderPath = Path.Combine(_environment.ContentRootPath, "Documents");
            var filePath = Path.Combine(folderPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File not found" });

            return PhysicalFile(filePath, "application/pdf", fileName);
        }
    }
}