using System.ComponentModel.DataAnnotations;
using Lazy.Application.Tags.SearchTag;

namespace Lazy.Presentation.Contracts.Posts;

public record UpdatePostRequest(
    [Required] string Title,
    [Required] string Summary,
    [Required] string Body,
    [Required] string Slug,
    string? CoverUrl,
    bool IsCoverDisplayed,
    List<Guid> Tags,
    bool IsPublished);