using Microsoft.AspNetCore.Identity;

namespace CyberLoot.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime JoinedOn { get; set; }
    }
}
