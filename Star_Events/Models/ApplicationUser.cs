using Microsoft.AspNetCore.Identity;

namespace Star_Events.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add additional properties if needed
        public string FirstName { get; set; } 
        public string LastName { get; set; }
        public int Age { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
    }
}
