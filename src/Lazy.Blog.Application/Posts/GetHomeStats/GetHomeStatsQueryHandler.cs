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

        MonthlyTopAuthor? topAuthor = await postRepository.GetMostActiveAuthorAsync(monthStart, now, ct);
        MonthlyTopPost? topPost = await postRepository.GetMostViewedPostAsync(monthStart, now, ct);
        IReadOnlyList<MonthlyPostCount> postsByMonth = await postRepository.GetMonthlyPostCountsAsync(ct);

        var response = new HomeStatsResponse(
            topAuthor?.ToMostActiveUserResponse(),
            topPost?.ToTopPostResponse(),
            postsByMonth.ToPostsPerMonth());

        return response;
    }
}
