using TransportApp_API.Models;

namespace TransportApp_API.DTOs.Subscriptions
{
    public class SubscriptionResponse
    {
        public int Id { get; set; }

        public string SubscriptionTypeId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string PurchasedAt { get; set; } = string.Empty;

        public string ExpiresAt { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public SubscriptionStatus Status { get; set; }

        public string UniqueCode { get; set; } = null!;
    }
}