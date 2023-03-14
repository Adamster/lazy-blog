using Lazy.Domain.Entities.Identity;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations.Identity;

public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable(TableNames.RoleClaims);

        builder.HasKey(x => x.Id);
    }
}