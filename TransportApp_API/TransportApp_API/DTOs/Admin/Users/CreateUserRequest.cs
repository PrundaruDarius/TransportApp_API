namespace TransportApp_API.DTOs.Admin.Users
{
    public class CreateUserRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}