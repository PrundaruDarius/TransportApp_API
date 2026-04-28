using TransportApp_API.Models;

namespace TransportApp_API.DTOs.Tickets
{
    public class TicketResponse
    {
        public int Id { get; set; }
        public DateTime PurchasedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; }
        public string UniqueCode { get; set; } = null!;
    }
}