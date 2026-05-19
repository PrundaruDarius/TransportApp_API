using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Services;
using TransportApp_API.DTOs.Admin.Stations;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/stations")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminStationsController : ControllerBase
    {
        private readonly JsonFileService _jsonService;

        public AdminStationsController(JsonFileService jsonService)
        {
            _jsonService = jsonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetStations()
        {
            var stations = await _jsonService.ReadJsonAsync<StationDto>("stations.json");
            return Ok(stations);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStation(CreateStationRequest request)
        {
            var stations = await _jsonService.ReadJsonAsync<StationDto>("stations.json");
            var newStation = new StationDto
            {
                Id = stations.Any() ? stations.Max(s => s.Id) + 1 : 1,
                LineId = request.LineId,
                Name = request.Name,
                Order = request.Order
            };
            stations.Add(newStation);
            await _jsonService.WriteJsonAsync("stations.json", stations);
            return Ok(newStation);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStation(int id, UpdateStationRequest request)
        {
            var stations = await _jsonService.ReadJsonAsync<StationDto>("stations.json");
            var station = stations.FirstOrDefault(s => s.Id == id);
            if (station == null) return NotFound();

            station.LineId = request.LineId;
            station.Name = request.Name;
            station.Order = request.Order;

            await _jsonService.WriteJsonAsync("stations.json", stations);
            return Ok(station);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStation(int id)
        {
            var stations = await _jsonService.ReadJsonAsync<StationDto>("stations.json");
            var station = stations.FirstOrDefault(s => s.Id == id);
            if (station == null) return NotFound();

            stations.Remove(station);
            await _jsonService.WriteJsonAsync("stations.json", stations);
            return Ok();
        }
    }
}