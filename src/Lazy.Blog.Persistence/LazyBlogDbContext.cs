using Lazy.Domain.Entities;
using Lazy.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lazy.Persistence;

public class LazyBlogDbContext : IdentityDbContext<
    User, Role, Guid,
    UserClaim, UserRole, UserLogin,
    RoleClaim, UserToken>
{
    public LazyBlogDbContext(DbContextOptions options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
}