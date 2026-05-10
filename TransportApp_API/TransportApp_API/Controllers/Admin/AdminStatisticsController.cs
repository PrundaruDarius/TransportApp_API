using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Data;
using TransportApp_API.DTOs.Admin.Statistics;
using TransportApp_API.Models;
using System.Linq;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminStatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminStatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("revenue/monthly")]
        public IActionResult GetMonthlyRevenue()
        {
            var tickets = _context.Tickets.Where(t => t.Status == TicketStatus.Used).ToList();
            var subscriptions = _context.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();

            var ticketGroups = tickets
                .GroupBy(t => t.PurchasedAt.ToString("yyyy-MM"))
                .Select(g => new { Month = g.Key, Revenue = g.Sum(t => t.Price) })
                .ToDictionary(g => g.Month, g => g.Revenue);

            var subscriptionGroups = subscriptions
                .GroupBy(s => s.PurchasedAt.ToString("yyyy-MM"))
                .Select(g => new { Month = g.Key, Revenue = g.Sum(s => s.Price) })
                .ToDictionary(g => g.Month, g => g.Revenue);

            var months = ticketGroups.Keys.Union(subscriptionGroups.Keys).OrderBy(m => m);

            var result = months.Select(m => new MonthlyRevenueDto
            {
                Month = m,
                TicketRevenue = ticketGroups.ContainsKey(m) ? ticketGroups[m] : 0,
                SubscriptionRevenue = subscriptionGroups.ContainsKey(m) ? subscriptionGroups[m] : 0
            }).ToList();

            return Ok(result);
        }
    }
}