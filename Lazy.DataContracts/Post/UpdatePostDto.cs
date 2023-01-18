namespace Lazy.DataContracts.Post;

public record UpdatePostDto(
    Guid Id,
    string Title,
    string? Description,
    string Content
    );