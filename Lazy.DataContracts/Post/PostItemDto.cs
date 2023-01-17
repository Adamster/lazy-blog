namespace Lazy.DataContracts.Post;

public record PostItemDto
(
     string Title ,
     string Description,
     string AuthorName,
     string Content,
     int LikeCount,
     int CommentsCount,
     DateTimeOffset DatePosted
);
