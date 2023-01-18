namespace Lazy.Domain;

public class Post : Entity
{
    public Post(string title, string? description, string content, Guid authorId)
    {
        Title = title;
        Description = description;
        Content = content;
        AuthorId = authorId;
    }

    public string Title { get; set; }
    public string? Description { get; set; }
    public string Content { get; set; }
    public Author Author { get; set; } = null!;
    public Guid AuthorId { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public int LikeCount { get; set; }

}