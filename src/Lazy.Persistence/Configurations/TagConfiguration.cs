using Lazy.Domain.Entities;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable(TableNames.Tags);

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Value).HasMaxLength(Tag.MaxLength);
            

        builder
            .HasMany(t => t.Posts)
            .WithMany(p => p.Tags);


        builder.HasIndex(t => t.Value).IsUnique();
    }
}