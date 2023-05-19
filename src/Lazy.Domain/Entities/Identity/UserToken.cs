using Lazy.Domain.Primitives;
using Microsoft.AspNetCore.Identity;

namespace Lazy.Domain.Entities.Identity;

public class UserToken : IdentityUserToken<Guid>, IAuditableEntity
{
    
    public DateTime ExpiryDate { get; private set; }

    public bool IsUsed { get; private set; }

    public bool IsInvalidated { get; private set; }
    public DateTime CreatedOnUtc { get; set; }

    public DateTime? UpdatedOnUtc { get; set; }
    public virtual User User { get; set; } = null!;
}