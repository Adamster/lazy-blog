using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Users.GetUserById;

namespace Lazy.Application.Posts.GetHomeStats;

public record HomeStatsResponse(
    MostActiveUserResponse? MostActiveUser,
    TopPostResponse? TopPost,
    IReadOnlyList<PostsPerMonth> PostsByMonth);

public record MostActiveUserResponse(
    UserResponse User,
    int PostCount,
    int NetRating);

public record TopPostResponse(
    string Title,
    string Slug,
    string UserName,
    long Views,
    int NetRating);
