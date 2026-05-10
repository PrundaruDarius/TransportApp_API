namespace TransportApp_API.DTOs.Admin.Announcements
{
    public class CreateAnnouncementRequest
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}