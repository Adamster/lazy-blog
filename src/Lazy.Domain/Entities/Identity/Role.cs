using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Entities.Identity;

public class Role : IdentityRole<Guid>
{
    public virtual ICollection<UserRole> UserRoles { get; set; }
    public virtual ICollection<RoleClaim> RoleClaims { get; set; }
}