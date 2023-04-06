﻿using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Domain.Entities;

public sealed class Comment : Entity, IAuditableEntity
{
    public const int MaxLength = 4000;

    public Comment(
        Guid id,
        Post post,
        User user,
        Body commentText)
    : base(id)
    {
        PostId = post.Id;
        UserId = user.Id;
        Post = post;
        User = user;
        CommentText = commentText;
    }

    private Comment()
    {
    }


    public Guid PostId { get; private set; }
    public Post Post { get; private set; }

    public Guid UserId { get; private set; }
    public User User { get; private set; }

    public Body CommentText { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }


    public static Comment Create(
        Post post,
        User user,
        Body commentText)
    {
        var comment = new Comment(Guid.NewGuid(), post, user, commentText);
        return comment;
    }
}