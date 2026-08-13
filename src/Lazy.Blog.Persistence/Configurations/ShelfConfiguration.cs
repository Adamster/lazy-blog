using Lazy.Domain.Entities;
using Lazy.Persistence.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lazy.Persistence.Configurations;

internal sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable(TableNames.Genres);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable(TableNames.Books);
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Genre).WithMany().HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Series).WithMany().HasForeignKey(x => x.SeriesId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TitleEn).HasMaxLength(300);
        builder.Property(x => x.CoverUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Rating).HasPrecision(4, 2);
        builder.Property(x => x.RatingSource).HasMaxLength(50);
        builder.Property(x => x.SourceWorkId).HasMaxLength(100);
        builder.Property(x => x.SourceUrl).HasMaxLength(1000);

        builder.HasIndex(x => new { x.RatingSource, x.SourceWorkId })
            .IsUnique()
            .HasFilter("[SourceWorkId] IS NOT NULL");
        builder.HasIndex(x => new { x.SeriesId, x.VolumeNumber }).IsUnique().HasFilter("[SeriesId] IS NOT NULL");
        builder.HasIndex(x => new { x.GenreId, x.AuthorId, x.Title });
    }
}

internal sealed class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    public void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.ToTable(TableNames.Series);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TitleEn).HasMaxLength(300);
        builder.HasIndex(x => x.Title).IsUnique();
    }
}

internal sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable(TableNames.Authors);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(300);

        builder.Property(x => x.NormalizedName).HasMaxLength(300).IsRequired();
        builder.HasIndex(x => x.NormalizedName).IsUnique();
    }
}

internal sealed class UserBookStatusConfiguration : IEntityTypeConfiguration<UserBookStatus>
{
    public void Configure(EntityTypeBuilder<UserBookStatus> builder)
    {
        builder.ToTable(TableNames.UserBookStatuses);
        builder.HasKey(x => new { x.UserId, x.BookId });

        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Status).HasConversion<byte>();
        builder.HasIndex(x => new { x.UserId, x.Status, x.UpdatedOnUtc });
    }
}

internal sealed class BookCommentConfiguration : IEntityTypeConfiguration<BookComment>
{
    public void Configure(EntityTypeBuilder<BookComment> builder)
    {
        builder.ToTable(TableNames.BookComments);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Body).HasMaxLength(BookComment.MaxLength).IsRequired();

        builder.HasOne(x => x.Book).WithMany().HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.BookId, x.CreatedOnUtc });
        builder.HasIndex(x => x.UserId);
    }
}

internal sealed class RatingSyncRunConfiguration : IEntityTypeConfiguration<RatingSyncRun>
{
    public void Configure(EntityTypeBuilder<RatingSyncRun> builder)
    {
        builder.ToTable(TableNames.RatingSyncRuns);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Source).HasMaxLength(50);
        builder.Property(x => x.Status).HasMaxLength(30);
        builder.Property(x => x.ErrorSummary).HasMaxLength(2000);

        builder.HasIndex(x => new { x.Source, x.CompletedOnUtc });
    }
}
