using Microsoft.AspNetCore.Identity;

namespace TransportApp_API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsDisabled { get; set; } = false;
    }
}