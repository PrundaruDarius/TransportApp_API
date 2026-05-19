namespace TransportApp_API.DTOs.Admin.Users
{
    public class UserResponse
    {
        public string Id { get; set; } = null!;
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsDisabled { get; set; }
    }
}