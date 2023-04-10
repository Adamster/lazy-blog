using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Posts.UpdatePost;
public record UpdatePostCommand(Guid Id, string Title, string Summary, string Body, string Slug, string? CoverUrl) : ICommand;