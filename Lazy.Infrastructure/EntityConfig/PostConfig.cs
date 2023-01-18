using Lazy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Infrastructure.EntityConfig;

internal sealed class PostConfig : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> post)
    {
        post.HasKey(x => x.Id);
        post.Property(x => x.Title);
        post.Property(x => x.Description);
        post.Property(x => x.Content);
        post.Property(x => x.LikeCount);

        post.HasOne(x => x.Author);
        post.HasMany(x => x.Comments);
    }
}