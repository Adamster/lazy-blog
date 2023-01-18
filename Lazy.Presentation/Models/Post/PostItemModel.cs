namespace Lazy.Presentation.Models.Post;

public record PostItemModel
(
    Guid Id,
    string Title,
    string Description,
    string Content,
    string AuthorName,
    int LikeCount,
    int CommentsCount,
    DateTimeOffset DatePosted
);