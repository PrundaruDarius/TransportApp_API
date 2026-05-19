using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportApp_API.Services;

namespace TransportApp_API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/statistics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminStatisticsController : ControllerBase
    {
        private readonly RevenueService _revenueService;

        public AdminStatisticsController(RevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        [HttpGet("revenue/monthly")]
        public async Task<IActionResult> GetMonthlyRevenue()
        {
            var revenue = await _revenueService.GetMonthlyRevenueAsync();

            var result = revenue
                .OrderBy(x => x.Month)
                .Select(x => new
                {
                    x.Month,
                    x.TicketRevenue,
                    x.SubscriptionRevenue,
                    x.TotalRevenue
                })
                .ToList();

            return Ok(result);
        }
    }
}