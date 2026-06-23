using Lazy.Application.Posts.Extensions;
using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;

namespace Lazy.Application.Posts.Shared;

public class UserPostResponseBuilder(
    IPostRepository postRepository,
    IPostVoteRepository postVoteRepository) : IUserPostResponseBuilder
{
    public async Task<UserPostResponse> BuildAsync(User user, IQueryable<Post> posts, CancellationToken ct)
    {
        List<UserPostItem> postsDetails = posts.ToUserPostItemResponse();

        Task<int> postCountTask = postRepository.GetPostCountByUserIdAsync(user.Id, ct);
        Task<VoteCounts> voteCountsTask = postVoteRepository.GetVoteCountsByAuthorIdAsync(user.Id, ct);
        Task<int> totalViewsTask = postRepository.GetTotalViewsByUserIdAsync(user.Id, ct);
        Task<IReadOnlyList<MonthlyPostCount>> monthlyPostCountsTask =
            postRepository.GetMonthlyPostCountsByUserIdAsync(user.Id, ct);

        await Task.WhenAll(postCountTask, voteCountsTask, totalViewsTask, monthlyPostCountsTask);

        VoteCounts voteCounts = voteCountsTask.Result;

        return new UserPostResponse(
            new UserResponse(user),
            postsDetails,
            postCountTask.Result,
            voteCounts.UpVotes,
            voteCounts.DownVotes,
            totalViewsTask.Result,
            monthlyPostCountsTask.Result.ToPostsPerMonth());
    }
}
