namespace TransportApp_API.DTOs.Admin.Statistics
{
    public class DashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalControllers { get; set; }
        public int ActiveTickets { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int ActiveLines { get; set; }
        public int InactiveLines { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }
}