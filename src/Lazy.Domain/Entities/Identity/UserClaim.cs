using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Entities.Identity;

public class UserClaim : IdentityUserClaim<Guid>
{
    public virtual User User { get; set; }
}