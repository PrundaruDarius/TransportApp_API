using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Services;
using TransportApp_API.DTOs.Admin.Announcements;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/announcements")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminAnnouncementsController : ControllerBase
    {
        private readonly JsonFileService _jsonService;

        public AdminAnnouncementsController(JsonFileService jsonService)
        {
            _jsonService = jsonService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnnouncements()
        {
            var announcements = await _jsonService.ReadJsonAsync<AnnouncementDto>("announcements.json");
            return Ok(announcements);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement(CreateAnnouncementRequest request)
        {
            var announcements = await _jsonService.ReadJsonAsync<AnnouncementDto>("announcements.json");
            var newAnnouncement = new AnnouncementDto
            {
                Id = announcements.Any() ? announcements.Max(a => a.Id) + 1 : 1,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };
            announcements.Add(newAnnouncement);
            await _jsonService.WriteJsonAsync("announcements.json", announcements);
            return Ok(newAnnouncement);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement(int id, UpdateAnnouncementRequest request)
        {
            var announcements = await _jsonService.ReadJsonAsync<AnnouncementDto>("announcements.json");
            var announcement = announcements.FirstOrDefault(a => a.Id == id);
            if (announcement == null) return NotFound();
            announcement.Title = request.Title;
            announcement.Content = request.Content;
            await _jsonService.WriteJsonAsync("announcements.json", announcements);
            return Ok(announcement);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcements = await _jsonService.ReadJsonAsync<AnnouncementDto>("announcements.json");
            var announcement = announcements.FirstOrDefault(a => a.Id == id);
            if (announcement == null) return NotFound();
            announcements.Remove(announcement);
            await _jsonService.WriteJsonAsync("announcements.json", announcements);
            return Ok();
        }
    }
}