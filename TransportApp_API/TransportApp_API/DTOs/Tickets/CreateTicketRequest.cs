namespace TransportApp_API.DTOs.Tickets
{
    public class CreateTicketRequest
    {
        public string PaymentMethod { get; set; } = "MockCard";
        
    }
}