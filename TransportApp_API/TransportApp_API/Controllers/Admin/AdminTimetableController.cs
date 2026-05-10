using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Services;
using TransportApp_API.DTOs.Admin.Timetable;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/timetable")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminTimetableController : ControllerBase
    {
        private readonly JsonFileService _jsonService;

        public AdminTimetableController(JsonFileService jsonService)
        {
            _jsonService = jsonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTimetable()
        {
            var timetable = await _jsonService.ReadJsonAsync<TimetableDto>("timetable.json");
            return Ok(timetable);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEntry(CreateTimetableRequest request)
        {
            var timetable = await _jsonService.ReadJsonAsync<TimetableDto>("timetable.json");
            var newEntry = new TimetableDto
            {
                StationId = request.StationId,
                LineCode = request.LineCode,
                Hour = request.Hour,
                Minutes = request.Minutes,
                IsActive = true
            };
            timetable.Add(newEntry);
            await _jsonService.WriteJsonAsync("timetable.json", timetable);
            return Ok(newEntry);
        }

        [HttpPut("{stationId}/{lineCode}/{hour}")]
        public async Task<IActionResult> UpdateEntry(int stationId, string lineCode, int hour, UpdateTimetableRequest request)
        {
            var timetable = await _jsonService.ReadJsonAsync<TimetableDto>("timetable.json");
            var entry = timetable.FirstOrDefault(t => t.StationId == stationId && t.LineCode == lineCode && t.Hour == hour);
            if (entry == null) return NotFound();
            entry.Minutes = request.Minutes;
            entry.IsActive = request.IsActive;
            await _jsonService.WriteJsonAsync("timetable.json", timetable);
            return Ok(entry);
        }

        [HttpDelete("{stationId}/{lineCode}/{hour}")]
        public async Task<IActionResult> DeleteEntry(int stationId, string lineCode, int hour)
        {
            var timetable = await _jsonService.ReadJsonAsync<TimetableDto>("timetable.json");
            var entry = timetable.FirstOrDefault(t => t.StationId == stationId && t.LineCode == lineCode && t.Hour == hour);
            if (entry == null) return NotFound();
            timetable.Remove(entry);
            await _jsonService.WriteJsonAsync("timetable.json", timetable);
            return Ok();
        }

        [HttpPut("activate/{stationId}/{lineCode}/{hour}")]
        public async Task<IActionResult> ActivateEntry(int stationId, string lineCode, int hour)
        {
            var timetable = await _jsonService.ReadJsonAsync<TimetableDto>("timetable.json");
            var entry = timetable.FirstOrDefault(t => t.StationId == stationId && t.LineCode == lineCode && t.Hour == hour);
            if (entry == null) return NotFound();
            entry.IsActive = true;
            await _jsonService.WriteJsonAsync("timetable.json", timetable);
            return Ok(entry);
        }

        [HttpPut("deactivate/{stationId}/{lineCode}/{hour}")]
        public async Task<IActionResult> DeactivateEntry(int stationId, string lineCode, int hour)
        {
            var timetable = await _jsonService.ReadJsonAsync<TimetableDto>("timetable.json");
            var entry = timetable.FirstOrDefault(t => t.StationId == stationId && t.LineCode == lineCode && t.Hour == hour);
            if (entry == null) return NotFound();
            entry.IsActive = false;
            await _jsonService.WriteJsonAsync("timetable.json", timetable);
            return Ok(entry);
        }
    }
}