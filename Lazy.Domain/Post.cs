namespace Lazy.Domain;

public class Post : Entity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public Author Author { get; set; }
}