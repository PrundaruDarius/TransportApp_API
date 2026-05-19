using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Services;
using TransportApp_API.DTOs.Admin.Lines;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/lines")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminLinesController : ControllerBase
    {
        private readonly JsonFileService _jsonService;

        public AdminLinesController(JsonFileService jsonService)
        {
            _jsonService = jsonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLines()
        {
            var lines = await _jsonService.ReadJsonAsync<LineDto>("lines.json");
            return Ok(lines);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLine(CreateLineRequest request)
        {
            var lines = await _jsonService.ReadJsonAsync<LineDto>("lines.json");
            var newLine = new LineDto
            {
                Id = lines.Any() ? lines.Max(l => l.Id) + 1 : 1,
                Code = request.Code,
                Name = request.Name,
                IsActive = true
            };
            lines.Add(newLine);
            await _jsonService.WriteJsonAsync("lines.json", lines);
            return Ok(newLine);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLine(int id, UpdateLineRequest request)
        {
            var lines = await _jsonService.ReadJsonAsync<LineDto>("lines.json");
            var line = lines.FirstOrDefault(l => l.Id == id);
            if (line == null) return NotFound();
            line.Code = request.Code;
            line.Name = request.Name;
            await _jsonService.WriteJsonAsync("lines.json", lines);
            return Ok(line);
        }

        [HttpPut("activate/{id}")]
        public async Task<IActionResult> ActivateLine(int id)
        {
            var lines = await _jsonService.ReadJsonAsync<LineDto>("lines.json");
            var line = lines.FirstOrDefault(l => l.Id == id);
            if (line == null) return NotFound();
            line.IsActive = true;
            await _jsonService.WriteJsonAsync("lines.json", lines);
            return Ok(line);
        }

        [HttpPut("deactivate/{id}")]
        public async Task<IActionResult> DeactivateLine(int id)
        {
            var lines = await _jsonService.ReadJsonAsync<LineDto>("lines.json");
            var line = lines.FirstOrDefault(l => l.Id == id);
            if (line == null) return NotFound();
            line.IsActive = false;
            await _jsonService.WriteJsonAsync("lines.json", lines);
            return Ok(line);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLine(int id)
        {
            var lines = await _jsonService.ReadJsonAsync<LineDto>("lines.json");
            var line = lines.FirstOrDefault(l => l.Id == id);
            if (line == null) return NotFound();
            lines.Remove(line);
            await _jsonService.WriteJsonAsync("lines.json", lines);
            return Ok();
        }
    }
}