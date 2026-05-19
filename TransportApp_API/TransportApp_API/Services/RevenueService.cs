using System.Text.Json;

namespace TransportApp_API.Services
{
    public class RevenueService
    {
        private readonly IWebHostEnvironment _environment;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public RevenueService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task AddTicketRevenueAsync(decimal amount, DateTime date)
        {
            await AddRevenueAsync(amount, 0, date);
        }

        public async Task AddSubscriptionRevenueAsync(decimal amount, DateTime date)
        {
            await AddRevenueAsync(0, amount, date);
        }

        public async Task<List<MonthlyRevenueItem>> GetMonthlyRevenueAsync()
        {
            var path = GetPath();

            if (!File.Exists(path))
                return new List<MonthlyRevenueItem>();

            var json = await File.ReadAllTextAsync(path);

            return JsonSerializer.Deserialize<List<MonthlyRevenueItem>>(json, _options)
                   ?? new List<MonthlyRevenueItem>();
        }

        public async Task<decimal> GetCurrentMonthTotalRevenueAsync()
        {
            var revenue = await GetMonthlyRevenueAsync();
            var currentMonth = DateTime.UtcNow.ToString("yyyy-MM");

            var item = revenue.FirstOrDefault(x => x.Month == currentMonth);

            return item?.TotalRevenue ?? 0;
        }

        private async Task AddRevenueAsync(decimal ticketAmount, decimal subscriptionAmount, DateTime date)
        {
            await _lock.WaitAsync();

            try
            {
                var path = GetPath();

                if (!File.Exists(path))
                    await File.WriteAllTextAsync(path, "[]");

                var json = await File.ReadAllTextAsync(path);

                var revenue = JsonSerializer.Deserialize<List<MonthlyRevenueItem>>(json, _options)
                              ?? new List<MonthlyRevenueItem>();

                var month = date.ToString("yyyy-MM");

                var item = revenue.FirstOrDefault(x => x.Month == month);

                if (item == null)
                {
                    item = new MonthlyRevenueItem
                    {
                        Month = month,
                        TicketRevenue = 0,
                        SubscriptionRevenue = 0
                    };

                    revenue.Add(item);
                }

                item.TicketRevenue += ticketAmount;
                item.SubscriptionRevenue += subscriptionAmount;

                revenue = revenue
                    .OrderBy(x => x.Month)
                    .ToList();

                var updatedJson = JsonSerializer.Serialize(revenue, _options);

                await File.WriteAllTextAsync(path, updatedJson);
            }
            finally
            {
                _lock.Release();
            }
        }

        private string GetPath()
        {
            return Path.Combine(_environment.ContentRootPath, "Data", "revenue.json");
        }
    }

    public class MonthlyRevenueItem
    {
        public string Month { get; set; } = string.Empty;

        public decimal TicketRevenue { get; set; }

        public decimal SubscriptionRevenue { get; set; }

        public decimal TotalRevenue => TicketRevenue + SubscriptionRevenue;
    }
}