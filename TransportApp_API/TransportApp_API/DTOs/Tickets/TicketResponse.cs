using TransportApp_API.Models;

namespace TransportApp_API.DTOs.Tickets
{
    public class TicketResponse
    {
        public int Id { get; set; }
        public string PurchasedAt { get; set; } = string.Empty;
        public string ExpiresAt { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; }
        public string UniqueCode { get; set; } = null!;
    }
}