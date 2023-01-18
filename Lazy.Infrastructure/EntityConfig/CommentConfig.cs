using Lazy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Infrastructure.EntityConfig;

public class CommentConfig : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> comment)
    {
        comment.HasKey(x => x.Id);

        comment.Property(x => x.Text);
        comment.Property(x => x.LikeCount);

        comment.HasOne(x => x.Author);
    }
}