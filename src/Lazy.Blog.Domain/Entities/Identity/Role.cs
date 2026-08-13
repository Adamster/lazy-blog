using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Entities.Identity;

public class Role : IdentityRole<Guid>
{
    public const string AdminRoleName = "Admin";
    public const string MemberRoleName = "Member";
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = null!;
    public virtual ICollection<RoleClaim> RoleClaims { get; set; } = null!;
}
