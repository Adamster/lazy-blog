using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Domain.Entities;

public sealed class Post : AggregateRoot, IAuditableEntity
{
    private readonly List<Comment> _comments = [];
    private readonly List<Tag> _tags = [];

    public Post(
        Guid id,
        Title title,
        Body body,
        Summary? summary,
        Slug slug,
        Guid userId,
        List<Tag> tags,
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
        _tags.AddRange(tags);
    }

    private Post()
    {
    }

    public Title Title { get; private set; } = null!;

    public Body Body { get; private set; } = null!;

    public Summary? Summary { get; private set; }

    public Slug Slug { get; private set; } = null!;

    public bool IsPublished { get; private set; }

    public string? CoverUrl { get; private set; }

    public long Views { get; private set; }

    public int Rating { get; private set; }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public IReadOnlyCollection<Comment> Comments => _comments;

    public IReadOnlyCollection<Tag> Tags => _tags;

    public static Post Create(
        Guid id,
        Title title,
        Summary? summary,
        Slug slug,
        Body body,
        Guid userId,
        bool isPublished,
        List<Tag> tags,
        string? coverUrl = null )
    {
        var post = new Post(
            id,
            title,
            body,
            summary,
            slug,
            userId,
            tags,
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

    public void Update(Title title,
        Summary? summary, 
        Body body,
        Slug slug, 
        string? coverUrl,
        List<Tag>? tags,
        bool isPublished)
    {
        Title = title;
        Summary = summary;
        Body = body;
        Slug = slug;
        CoverUrl = coverUrl ?? string.Empty;
        IsPublished = isPublished;
        
        if (tags is not null)
        {
            _tags.Clear();
            _tags.AddRange(tags);
        }
    }

    private void UpVote() => Rating++;

    private void DownVote() => Rating--;

    public void Vote(VoteDirection postVoteVoteDirection)
    {
        switch (postVoteVoteDirection)
        {
            case VoteDirection.Up:
                UpVote();
                break;
            case VoteDirection.Down:
                DownVote();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(postVoteVoteDirection), postVoteVoteDirection, null);
        }
    }

    public void Hide() => IsPublished = false;

    public void Publish() => IsPublished = true;
}