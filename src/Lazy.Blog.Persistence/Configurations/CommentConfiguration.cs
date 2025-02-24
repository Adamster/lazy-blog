using Lazy.Domain.Entities;
using Lazy.Domain.ValueObjects.Post;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable(TableNames.Comments);

        builder.HasKey(c => c.Id);

        builder.Property(x => x.CommentText)
            .HasConversion(x => x.Value, v => Body.Create(v).Value);
    }
}