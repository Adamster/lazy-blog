using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Entities.Identity;

public class UserToken : IdentityUserToken<Guid>
{
    public virtual User User { get; set; } = null!;
}