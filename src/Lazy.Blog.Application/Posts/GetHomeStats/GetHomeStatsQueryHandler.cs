using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.Extensions;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetHomeStats;

public class GetHomeStatsQueryHandler(IPostRepository postRepository)
    : IQueryHandler<GetHomeStatsQuery, HomeStatsResponse>
{
    public async Task<Result<HomeStatsResponse>> Handle(GetHomeStatsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        Task<MonthlyTopAuthor?> topAuthorTask = postRepository.GetMostActiveAuthorAsync(monthStart, now, ct);
        Task<MonthlyTopPost?> topPostTask = postRepository.GetMostViewedPostAsync(monthStart, now, ct);
        Task<IReadOnlyList<MonthlyPostCount>> postsByMonthTask = postRepository.GetMonthlyPostCountsAsync(ct);

        await Task.WhenAll(topAuthorTask, topPostTask, postsByMonthTask);

        MonthlyTopAuthor? topAuthor = topAuthorTask.Result;
        MonthlyTopPost? topPost = topPostTask.Result;

        var response = new HomeStatsResponse(
            topAuthor?.ToMostActiveUserResponse(),
            topPost?.ToTopPostResponse(),
            postsByMonthTask.Result.ToPostsPerMonth());

        return response;
    }
}
