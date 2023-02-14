namespace Lazy.Presentation.Contracts.Posts;

public record CreatePostRequest(string Title, string Summary, string Body, Guid UserId);