namespace Lazy.Presentation.Models.Post;

public record PostItemModel
(
    string Title,
    string Description,
    string Content,
    string AuthorName,
    int LikeCount,
    int CommentsCount,
    DateTimeOffset DatePosted
);