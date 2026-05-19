namespace TransportApp_API.DTOs.Admin.Statistics
{
    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = null!;
        public decimal TicketRevenue { get; set; }
        public decimal SubscriptionRevenue { get; set; }
        public decimal TotalRevenue => TicketRevenue + SubscriptionRevenue;
    }
}