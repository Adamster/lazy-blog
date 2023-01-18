namespace Lazy.DataContracts.Post;

public record CreatePostDto(string Title, string? Description, string Content, Guid AuthorId);