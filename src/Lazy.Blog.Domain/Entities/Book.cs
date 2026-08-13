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
    public Guid AuthorId { get; private set; }
    public Author Author { get; private set; } = null!;
    public string? Series { get; private set; }
    public string? SeriesTitlesJson { get; private set; }
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
        => ValidateCatalog(slug, title, coverUrl, null, null, null, null).IsFailure
            ? Result.Failure<Book>(DomainErrors.Shelf.InvalidData("Book catalog data is empty or too long"))
            : new Book(id, genreId, authorId, slug, title, coverUrl);
    public Result UpdateRating(decimal rating, int ratingCount, string source, DateTime syncedOnUtc)
    {
        if (rating is < 0 or > 10 || ratingCount < 0 || string.IsNullOrWhiteSpace(source) || source.Length > 50)
            return Result.Failure(DomainErrors.Shelf.InvalidData("Book rating data is invalid"));
        (Rating, RatingCount, RatingSource, RatingSyncedOnUtc) = (rating, ratingCount, source, syncedOnUtc);
        return Result.Success();
    }
    public Result UpdateCatalog(Guid genreId, Guid authorId, string title, string coverUrl,
        string? series, string? seriesTitlesJson, string? sourceWorkId, string? sourceUrl)
    {
        var valid = ValidateCatalog(Slug, title, coverUrl, series, sourceWorkId, sourceUrl, seriesTitlesJson);
        if (valid.IsFailure) return valid;
        (GenreId, AuthorId, Title, CoverUrl, Series, SeriesTitlesJson, SourceWorkId, SourceUrl) =
            (genreId, authorId, title, coverUrl, series, seriesTitlesJson, sourceWorkId, sourceUrl);
        return Result.Success();
    }
    public Result UpdateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 160)
            return Result.Failure(DomainErrors.Shelf.InvalidData("Book slug must contain 1 to 160 characters"));
        Slug = slug;
        return Result.Success();
    }
    private static Result ValidateCatalog(string slug, string title, string coverUrl, string? series, string? sourceWorkId, string? sourceUrl, string? seriesTitlesJson)
        => string.IsNullOrWhiteSpace(slug) || slug.Length > 160 || string.IsNullOrWhiteSpace(title) || title.Length > 300 || string.IsNullOrWhiteSpace(coverUrl) || coverUrl.Length > 1000 || series?.Length > 300 || sourceWorkId?.Length > 100 || sourceUrl?.Length > 1000 || seriesTitlesJson?.Length > 100_000
            ? Result.Failure(DomainErrors.Shelf.InvalidData("Book catalog data is empty or too long")) : Result.Success();
}
