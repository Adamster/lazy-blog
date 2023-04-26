﻿using Lazy.Domain.Primitives;
using Lazy.Domain.ValueObjects.Post;
using static Lazy.Domain.Errors.DomainErrors;

namespace Lazy.Domain.Entities;

public class PostVote : Entity, IAuditableEntity
{
    public PostVote(Guid id, Post post, User user, VoteDirection voteDirection)
        : base(id)
    {
        Post = post;
        User = user;
        VoteDirection = voteDirection;
    }

    private PostVote()
    {
    }

    public Guid PostId { get; private set; }
    public Post Post { get; private set; }


    public Guid UserId { get; private set; }
    public User User { get; private set; }

    public VoteDirection VoteDirection { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }

    public static PostVote Create(Post post, User currentUser, VoteDirection direction)
    {
        post.Vote(direction);

        var postVote = new PostVote(Guid.NewGuid(), post, currentUser, direction);
        return postVote;
    }

    public bool Update(VoteDirection direction)
    {
        if (VoteDirection == direction)
        {
            return false;
        }
        
        Post.Vote(direction);

        VoteDirection = direction;

        return true;
    }
}