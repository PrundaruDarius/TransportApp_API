namespace TransportApp_API.DTOs.Subscriptions
{
    public class ValidateSubscriptionResponse
    {
        public bool Valid { get; set; }

        public string Message { get; set; } = string.Empty;

        public int? SubscriptionId { get; set; }

        public string? Status { get; set; }

        public string? ExpiresAt { get; set; }
    }
}