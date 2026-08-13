using Lazy.Domain.Primitives;
using Lazy.Domain.Errors;
using Lazy.Domain.Shared;

namespace Lazy.Domain.Entities;

public sealed class Book : Entity, IAuditableEntity
{
    private Book() { }

    private Book(Guid id, Guid genreId, Guid authorId, string slug, string title, string coverUrl) : base(id)
        => (GenreId, AuthorId, Slug, Title, CoverUrl) = (genreId, authorId, slug, title, coverUrl);

    public Guid GenreId { get; private set; }
    public Genre Genre { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? TitleEn { get; private set; }
    public Guid AuthorId { get; private set; }
    public Author Author { get; private set; } = null!;
    public Guid? SeriesId { get; private set; }
    public Series? Series { get; private set; }
    public int? VolumeNumber { get; private set; }
    public string CoverUrl { get; private set; } = null!;
    public decimal Rating { get; private set; }
    public int RatingCount { get; private set; }
    public string RatingSource { get; private set; } = "FantLab";
    public string? SourceWorkId { get; private set; }
    public string? SourceUrl { get; private set; }
    public DateTime? RatingSyncedOnUtc { get; private set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public static Result<Book> Create(Guid id, Guid genreId, Guid authorId, string slug, string title, string coverUrl)
        => ValidateCatalog(slug, title, null, coverUrl, null, null, null, null).IsFailure
            ? Result.Failure<Book>(DomainErrors.Shelf.InvalidData("Book catalog data is empty or too long"))
            : new Book(id, genreId, authorId, slug, title, coverUrl);

    public Result UpdateRating(decimal rating, int ratingCount, string source, DateTime syncedOnUtc)
    {
        if (rating is < 0 or > 10 || ratingCount < 0 || string.IsNullOrWhiteSpace(source) || source.Length > 50)
            return Result.Failure(DomainErrors.Shelf.InvalidData("Book rating data is invalid"));

        (Rating, RatingCount, RatingSource, RatingSyncedOnUtc) = (rating, ratingCount, source, syncedOnUtc);
        return Result.Success();
    }

    public Result UpdateCatalog(Guid genreId, Guid authorId, string title, string? titleEn, string coverUrl,
        Guid? seriesId, int? volumeNumber, string? sourceWorkId, string? sourceUrl)
    {
        var valid = ValidateCatalog(Slug, title, titleEn, coverUrl, seriesId, volumeNumber, sourceWorkId, sourceUrl);
        if (valid.IsFailure) return valid;

        (GenreId, AuthorId, Title, TitleEn, CoverUrl, SeriesId, VolumeNumber, SourceWorkId, SourceUrl) =
            (genreId, authorId, title, titleEn, coverUrl, seriesId, volumeNumber, sourceWorkId, sourceUrl);
        return Result.Success();
    }

    public Result UpdateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 160)
            return Result.Failure(DomainErrors.Shelf.InvalidData("Book slug must contain 1 to 160 characters"));

        Slug = slug;
        return Result.Success();
    }

    private static Result ValidateCatalog(
        string slug,
        string title,
        string? titleEn,
        string coverUrl,
        Guid? seriesId,
        int? volumeNumber,
        string? sourceWorkId,
        string? sourceUrl)
    {
        bool invalidText =
            string.IsNullOrWhiteSpace(slug) ||
            slug.Length > 160 ||
            string.IsNullOrWhiteSpace(title) ||
            title.Length > 300 ||
            titleEn?.Length > 300 ||
            string.IsNullOrWhiteSpace(coverUrl) ||
            coverUrl.Length > 1000 ||
            sourceWorkId?.Length > 100 ||
            sourceUrl?.Length > 1000;
        bool incompleteSeries = seriesId.HasValue != volumeNumber.HasValue;
        return invalidText || incompleteSeries
            ? Result.Failure(DomainErrors.Shelf.InvalidData(
                "Book catalog data is empty, too long, or has a series without a volume number"))
            : Result.Success();
    }
}
