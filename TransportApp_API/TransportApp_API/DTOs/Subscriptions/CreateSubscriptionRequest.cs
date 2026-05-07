namespace TransportApp_API.DTOs.Subscriptions
{
    public class CreateSubscriptionRequest
    {
        public string SubscriptionTypeId { get; set; } = "s1";

        public string PaymentMethod { get; set; } = "MockCard";
    }
}