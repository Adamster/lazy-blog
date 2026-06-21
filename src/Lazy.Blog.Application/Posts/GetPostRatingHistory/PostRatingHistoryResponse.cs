namespace Lazy.Application.Posts.GetPostRatingHistory;

public record PostRatingHistoryResponse(
    Guid PostId,
    string Slug,
    int Rating,
    int UpVotes,
    int DownVotes,
    IReadOnlyList<PostRatingHistoryPoint> Series);

public record PostRatingHistoryPoint(
    DateTime TimestampUtc,
    int CumulativeRating);
