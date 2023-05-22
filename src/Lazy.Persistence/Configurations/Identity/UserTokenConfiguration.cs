using Lazy.Domain.Entities.Identity;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations.Identity;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable(TableNames.Tokens);

        builder.HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

        builder.Property(x => x.JwtId);
        builder.Property(x => x.ExpiryDate);
        builder.Property(x => x.IsInvalidated);
        builder.Property(x => x.IsUsed);
        builder.Property(x => x.CreatedOnUtc);
        builder.Property(x => x.UpdatedOnUtc);
    }
}