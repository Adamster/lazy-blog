namespace Lazy.Domain;

public class Comment : Entity
{
    public Comment(string text)
    {
        Text = text;
        LikeCount = 0;
    }

    public string Text { get; set; }


    public int LikeCount { get; set; }

    public Author Author { get; set; } = null!;
}