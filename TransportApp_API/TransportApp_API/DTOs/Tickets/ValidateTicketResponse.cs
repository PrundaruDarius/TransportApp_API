namespace TransportApp_API.DTOs.Tickets
{
    public class ValidateTicketResponse
    {
        public bool Valid { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? TicketId { get; set; }
        public string? Status { get; set; }
        public string? ExpiresAt { get; set; }
    }
}