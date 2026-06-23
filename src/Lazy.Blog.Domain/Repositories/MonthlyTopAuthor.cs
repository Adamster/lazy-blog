using Lazy.Domain.Entities;

namespace Lazy.Domain.Repositories;

public sealed record MonthlyTopAuthor(User User, int PostCount, int NetRating);
