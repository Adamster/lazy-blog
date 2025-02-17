using System.ComponentModel.DataAnnotations;
using Lazy.Application.Tags.SearchTag;

namespace Lazy.Presentation.Contracts.Posts;

public record CreatePostRequest(
    [Required]string Title,
    [Required]string Summary,
    [Required]string Body,
    [Required]Guid UserId,
    List<TagResponse>? Tags,
    string? CoverUrl,
    bool IsPublished = true);