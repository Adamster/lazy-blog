using Lazy.Domain.Entities;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

public sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> b)
    {
        b.ToTable(TableNames.Genres); b.HasKey(x => x.Id);
        b.Property(x => x.Slug).HasMaxLength(50).IsRequired(); b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired(); b.HasIndex(x => x.Name).IsUnique();
    }
}

public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> b)
    {
        b.ToTable(TableNames.Books); b.HasKey(x => x.Id);
        b.HasOne(x => x.Genre).WithMany().HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Slug).HasMaxLength(160).IsRequired(); b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Series).HasMaxLength(300); b.Property(x => x.CoverUrl).HasMaxLength(1000).IsRequired();
        b.Property(x => x.SeriesTitlesJson).HasColumnType("nvarchar(max)");
        b.Property(x => x.Rating).HasPrecision(4, 2); b.Property(x => x.RatingSource).HasMaxLength(50);
        b.Property(x => x.SourceWorkId).HasMaxLength(100); b.Property(x => x.SourceUrl).HasMaxLength(1000);
        b.HasIndex(x => new { x.RatingSource, x.SourceWorkId }).IsUnique().HasFilter("[SourceWorkId] IS NOT NULL");
        b.HasIndex(x => new { x.GenreId, x.AuthorId, x.Title });
    }
}

public sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{ public void Configure(EntityTypeBuilder<Author> b) { b.ToTable(TableNames.Authors); b.HasKey(x => x.Id); b.Property(x => x.Name).HasMaxLength(300).IsRequired(); b.Property(x => x.NormalizedName).HasMaxLength(300).IsRequired(); b.HasIndex(x => x.NormalizedName).IsUnique(); } }

public sealed class UserBookStatusConfiguration : IEntityTypeConfiguration<UserBookStatus>
{
    public void Configure(EntityTypeBuilder<UserBookStatus> b)
    {
        b.ToTable(TableNames.UserBookStatuses); b.HasKey(x => new { x.UserId, x.BookId });
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.Status).HasConversion<byte>();
        b.HasIndex(x => new { x.UserId, x.Status, x.UpdatedOnUtc });
    }
}

public sealed class BookCommentConfiguration : IEntityTypeConfiguration<BookComment>
{
    public void Configure(EntityTypeBuilder<BookComment> b)
    {
        b.ToTable(TableNames.BookComments); b.HasKey(x => x.Id);
        b.Property(x => x.Body).HasMaxLength(BookComment.MaxLength).IsRequired();
        b.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.BookId, x.CreatedOnUtc }); b.HasIndex(x => x.UserId);
    }
}

public sealed class RatingSyncRunConfiguration : IEntityTypeConfiguration<RatingSyncRun>
{
    public void Configure(EntityTypeBuilder<RatingSyncRun> b)
    {
        b.ToTable(TableNames.RatingSyncRuns); b.HasKey(x => x.Id);
        b.Property(x => x.Source).HasMaxLength(50); b.Property(x => x.Status).HasMaxLength(30);
        b.Property(x => x.ErrorSummary).HasMaxLength(2000);
        b.HasIndex(x => new { x.Source, x.CompletedOnUtc });
    }
}
