using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Domain.Entities;

public class PostVote : Entity, IAuditableEntity
{
    public Guid PostId { get; private set; }
    public Post Post { get; private set; }


    public Guid UserId { get; private set; }
    public User User { get; private set; }

    public VoteDirection VoteDirection { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
}