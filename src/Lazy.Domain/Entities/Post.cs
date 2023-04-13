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
        Summary? summary,
        Slug slug,
        Guid userId,
        bool isPublished = true,
        string? coverUrl = null) : base(id)
    {
        Title = title;
        Body = body;
        Summary = summary;
        UserId = userId;
        Slug = slug;
        IsPublished = isPublished;
        CoverUrl = coverUrl ?? string.Empty;
        //_tags.AddRange(tags);
    }

    public Title Title { get; private set; }

    public Body Body { get; private set; }

    public Summary? Summary { get; private set; }

    public Slug Slug { get; private set; }

    public bool IsPublished { get; private set; }

    public string? CoverUrl { get; private set; }

    public long Views { get; private set; }

    public Guid UserId { get; private set; }

    public User User { get; private set; }

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public IReadOnlyCollection<Comment> Comments => _comments;

    //public IReadOnlyCollection<Tag> Tags => _tags;

    public static Post Create(
        Guid id,
        Title title,
        Summary? summary,
        Slug slug,
        Body body,
        Guid userId,
        bool isPublished,
        string? coverUrl = null
        /*params Tag[] tags*/)
    {
        var post = new Post(
            id,
            title,
            body,
            summary,
            slug,
            userId,
            isPublished,
            coverUrl);

        return post;
    }

    public void AddView()
    {
        if (!IsPublished)
        {
            return;
        }

        Views++;
    }

    public void Update(Title title, Summary? summary, Body body, Slug slug, string? coverUrl)
    {
        Title = title;
        Summary = summary;
        Body = body;
        Slug = slug;
        CoverUrl = coverUrl ?? string.Empty;
    }
}