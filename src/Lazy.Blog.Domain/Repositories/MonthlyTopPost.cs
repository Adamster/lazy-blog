namespace Lazy.Domain.Repositories;

public sealed record MonthlyTopPost(string Title, string Slug, string UserName, long Views, int NetRating);
