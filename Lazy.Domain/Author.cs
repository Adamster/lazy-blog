    namespace Lazy.Domain;

public class Author : Entity
{
    public Author(Guid id, string name, string webUrl, string email)
    {
        Id = id;
        Name = name;
        WebUrl = webUrl;
        Email = email;
    }
    
    public Author(string name, string webUrl, string email)
    {
        Name = name;
        WebUrl = webUrl;
        Email = email;
    }

    public string Name { get; set; }

    public string Email { get; set; }

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