namespace Lazy.Presentation.Contracts.Posts;

public record UpdatePostRequest(string Title, string Summary, string Body, string Slug);