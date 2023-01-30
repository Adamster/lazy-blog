using Lazy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Infrastructure.EntityConfig;

internal sealed class AuthorConfig : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> author)
    {
        author.HasKey(x => x.Id);
        author.Property(x => x.Name)
            .IsRequired();

        author.Property(x => x.WebUrl);

        author.Property(x => x.Email)
            .HasComputedColumnSql("[Name] + '@notlazy.blog'")
            .IsRequired();

        author.HasMany(x => x.Posts);
        author.HasMany(x => x.Comments);

        author.HasData(new Author(Constants.SystemAuthor.SystemAuthorId, "System", "system", "admin@notlazy.blog"));
    }
}