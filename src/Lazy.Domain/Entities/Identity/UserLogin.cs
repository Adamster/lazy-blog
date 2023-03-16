using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Entities.Identity;

public class UserLogin : IdentityUserLogin<Guid>
{
    public virtual User User { get; set; }
}