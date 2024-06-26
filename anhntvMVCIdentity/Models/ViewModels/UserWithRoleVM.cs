using anhntvMVCIdentity.Models.Entities;

namespace anhntvMVCIdentity.Models.ViewModels
{
    public class UserWithRoleVM
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
    }
}