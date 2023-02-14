using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Domain.Entities;

public sealed class Post : AggregateRoot, IAuditableEntity
{
    private readonly List<Comment> _comments = new();
    //private readonly List<Tag> _tags = new();

    public Post(
        Guid id,
        Title title,
        Body body,
        Summary summary,
        Guid userId) : base(id)
    {
        Title = title;
        Body = body;
        Summary = summary;
        UserId = userId;
        IsPublished = true;
        //_tags.AddRange(tags);
    }

    public Title Title { get; set; }

    public Body Body { get; set; }

    public Summary Summary { get; set; }

    public bool IsPublished { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public IReadOnlyCollection<Comment> Comments => _comments;

    //public IReadOnlyCollection<Tag> Tags => _tags;

    public static Post Create(
        Guid id,
        Title title,
        Summary summary,
        Body body,
        Guid userId
        /*params Tag[] tags*/)
    {
        var post = new Post(
            id,
            title,
            body,
            summary,
            userId);

        return post;
    }

    public void AddComment(Comment comment)
    {
        _comments.Add(comment);
    }

    public void Update(Title title, Summary summary, Body body)
    {
        Title = title;
        Summary = summary;
        Body = body;
    }
}