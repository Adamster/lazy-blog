using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.GetPostRatingHistory;

public class GetPostRatingHistoryHandler(
    IPostRepository postRepository,
    IPostVoteRepository postVoteRepository)
    : IQueryHandler<GetPostRatingHistoryQuery, PostRatingHistoryResponse>
{
    private const int MaxPoints = 12;

    public async Task<Result<PostRatingHistoryResponse>> Handle(
        GetPostRatingHistoryQuery request,
        CancellationToken ct)
    {
        Result<Slug> slugResult = Slug.Create(request.Slug);

        if (slugResult.IsFailure)
        {
            return Result.Failure<PostRatingHistoryResponse>(slugResult.Error);
        }

        Post? post = await postRepository.GetBySlugAsync(slugResult.Value, ct);

        if (post is null)
        {
            return Result.Failure<PostRatingHistoryResponse>(
                DomainErrors.Post.SlugNotFound(slugResult.Value));
        }

        List<VoteEntry> votes = postVoteRepository
            .GetPostVotesByPostId(post.Id, ct)
            .Select(pv => new VoteEntry(
                pv.CreatedOnUtc,
                pv.VoteDirection == VoteDirection.Up ? 1 : -1))
            .ToList();

        int upVotes = votes.Count(v => v.Delta > 0);
        int downVotes = votes.Count(v => v.Delta < 0);

        IReadOnlyList<PostRatingHistoryPoint> series =
            BuildSeries(votes, post.Rating, post.CreatedOnUtc);

        return new PostRatingHistoryResponse(
            post.Id,
            post.Slug.Value,
            post.Rating,
            upVotes,
            downVotes,
            series);
    }

    private static IReadOnlyList<PostRatingHistoryPoint> BuildSeries(
        IReadOnlyList<VoteEntry> votes,
        int currentRating,
        DateTime postCreatedOnUtc)
    {
        if (votes.Count == 0)
        {
            return [new PostRatingHistoryPoint(postCreatedOnUtc, currentRating)];
        }

        var cumulative = new List<PostRatingHistoryPoint>(votes.Count);
        int running = 0;

        foreach (VoteEntry vote in votes)
        {
            running += vote.Delta;
            cumulative.Add(new PostRatingHistoryPoint(vote.CreatedOnUtc, running));
        }

        List<PostRatingHistoryPoint> sampled = Sample(cumulative, MaxPoints);

        sampled[^1] = sampled[^1] with { CumulativeRating = currentRating };

        return sampled;
    }

    private static List<PostRatingHistoryPoint> Sample(
        IReadOnlyList<PostRatingHistoryPoint> points,
        int maxPoints)
    {
        if (points.Count <= maxPoints)
        {
            return [.. points];
        }

        var sampled = new List<PostRatingHistoryPoint>(maxPoints);

        for (int i = 0; i < maxPoints; i++)
        {
            int index = (int)((long)i * (points.Count - 1) / (maxPoints - 1));
            sampled.Add(points[index]);
        }

        return sampled;
    }

    private readonly record struct VoteEntry(DateTime CreatedOnUtc, int Delta);
}
