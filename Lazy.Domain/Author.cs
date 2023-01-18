namespace Lazy.Domain;

public class Author : Entity
{

    public Author(string name, string webUrl)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        WebUrl = webUrl ?? throw new ArgumentNullException(nameof(webUrl));
    }

    public Author(Guid id, string name, string webUrl)
    {
        Id = id;
        Name = name;
        WebUrl = webUrl;
    }

    public string Name { get; set; }

    public string WebUrl { get; set; }

    public ICollection<Post> Posts { get; } = new List<Post>();

    public ICollection<Comment> Comments { get; } = new List<Comment>();


    public void Update(string updatedAuthorName, string updatedAuthorWebUrl)
    {
        Name = updatedAuthorName ?? throw new ArgumentNullException(nameof(updatedAuthorName));
        WebUrl = updatedAuthorWebUrl ?? throw new ArgumentNullException(nameof(updatedAuthorWebUrl));
        UpdatedAt = DateTime.Now;
    }
}