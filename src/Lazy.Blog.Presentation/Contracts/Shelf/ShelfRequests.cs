using Microsoft.AspNetCore.Http;
namespace Lazy.Presentation.Contracts.Shelf;

public record SetBookStatusRequest(string Status);
public record BookCommentRequest(string Body);
public record RatingImportItemRequest(string SourceWorkId, decimal Rating, int RatingCount);
public record ImportBookRatingsRequest(string Source, IReadOnlyList<RatingImportItemRequest> Items);
public record ManageGenreRequest(string Slug,string Name,int SortOrder);
public record ManageAuthorRequest(string Name);
public sealed class ManageBookFormRequest
{
    public Guid GenreId { get; init; }
    public Guid AuthorId { get; init; }
    public string Slug { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string? Series { get; init; }
    public List<string> SeriesTitles { get; init; } = [];
    public decimal Rating { get; init; }
    public int RatingCount { get; init; }
    public string RatingSource { get; init; } = "FantLab";
    public string? SourceWorkId { get; init; }
    public string? SourceUrl { get; init; }
    public IFormFile? Cover { get; init; }
}
