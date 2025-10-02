using Microsoft.AspNetCore.Identity;

namespace BoardHoard.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
    }

}
