using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Posts.Shared;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Posts.GetPostByUserName;

public class GetPostByUserNameQueryHandler(
    IPostRepository postRepository,
    IUserRepository userRepository,
    IUserPostResponseBuilder userPostResponseBuilder,
    ICurrentUserContext currentUserContext)
    : IQueryHandler<GetPostByUserNameQuery, UserPostResponse>
{
    public async Task<Result<UserPostResponse>> Handle(GetPostByUserNameQuery request, CancellationToken ct)
    {
        var userNameResult = UserName.Create(request.UserName);
        var user = await userRepository.GetByUsernameAsync(userNameResult.Value, ct);

        if (user is null)
        {
            return Result.Failure<UserPostResponse>(new Error(
                "User.NotFound",
                $"The user with Username {request.UserName} was not found."));
        }

        var includeDraftPosts = currentUserContext.GetCurrentUserId() == user.Id;

        var posts = postRepository.GetPostsByUserName(userNameResult.Value, request.Offset, ct, includeDraftPosts);

        return await userPostResponseBuilder.BuildAsync(user, posts, ct);
    }
}