namespace Lazy.Domain;

public class Author : Entity
{
    public Author(string name, string webUrl)
    {
        Name = name;
        WebUrl = webUrl;
    }

    public string Name { get; set; }

    public string WebUrl { get; set; }

    public ICollection<Post> Posts { get; } = new List<Post>();
}