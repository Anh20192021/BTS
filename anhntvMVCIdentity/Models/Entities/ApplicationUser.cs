using Microsoft.AspNetCore.Identity;

namespace anhntvMVCIdentity.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? FullName { get; set; }
    }
}