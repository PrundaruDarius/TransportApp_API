using Microsoft.EntityFrameworkCore;
using TransportApp_API.Data;
using TransportApp_API.Models;

namespace TransportApp_API.Services
{
    public class TicketCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public TicketCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.UtcNow;

                var ticketsToDelete = await context.Tickets
                    .Where(t =>
                    t.Status == TicketStatus.Cancelled ||
                    t.Status == TicketStatus.Expired ||
                    (t.Status == TicketStatus.Used && t.ExpiresAt < now))
               .ToListAsync(stoppingToken);

                if (ticketsToDelete.Any())
                {
                    context.Tickets.RemoveRange(ticketsToDelete);
                    await context.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}