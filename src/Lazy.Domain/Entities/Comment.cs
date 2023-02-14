using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Domain.Entities;

public sealed class Comment : Entity, IAuditableEntity
{
    public Comment(
        Guid id,
        Post post,
        User user,
        Body commentText)
    : base(id)
    {
        PostId = post.Id;
        UserId = user.Id;
        CommentText = commentText;
    }

    private Comment()
    {
    }


    public Guid PostId { get; private set; }

    public Guid UserId { get; private set; }

    public Body CommentText { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}