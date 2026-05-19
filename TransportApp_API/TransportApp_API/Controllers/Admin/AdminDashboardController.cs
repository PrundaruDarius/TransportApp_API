using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Data;
using TransportApp_API.DTOs.Admin.Statistics;
using TransportApp_API.Models;
using TransportApp_API.Services;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly JsonFileService _jsonService;
        private readonly AppDbContext _context;

        public AdminDashboardController(JsonFileService jsonService, AppDbContext context)
        {
            _jsonService = jsonService;
            _context = context;
        }

        [HttpGet]
        public IActionResult GetDashboard()
        {
            var totalUsers = _context.Users.Count();
            var totalControllers = _context.Users
                .Count(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id &&
                               ur.RoleId == _context.Roles.First(r => r.Name == "Controller").Id));

            var activeTickets = _context.Tickets.Count(t => t.Status == TicketStatus.Used);
            var activeSubscriptions = _context.Subscriptions.Count(s => s.Status == SubscriptionStatus.Active);

            var lines = _jsonService.ReadJsonAsync<DTOs.Admin.Lines.LineDto>("lines.json").Result;
            var activeLines = lines.Count(l => l.IsActive);
            var inactiveLines = lines.Count(l => !l.IsActive);

            var stations = _jsonService.ReadJsonAsync<DTOs.Admin.Stations.StationDto>("stations.json").Result;

            var monthlyRevenue = _context.Tickets.Where(t => t.Status == TicketStatus.Used).Sum(t => t.Price) +
                                 _context.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active).Sum(s => s.Price);

            var dashboard = new DashboardDto
            {
                TotalUsers = totalUsers,
                TotalControllers = totalControllers,
                ActiveTickets = activeTickets,
                ActiveSubscriptions = activeSubscriptions,
                ActiveLines = activeLines,
                InactiveLines = inactiveLines,
                MonthlyRevenue = monthlyRevenue
            };

            return Ok(dashboard);
        }
    }
}