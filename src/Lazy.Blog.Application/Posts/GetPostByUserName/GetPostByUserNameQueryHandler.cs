using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.Extensions;
using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Posts.GetPostByUserName;

public class GetPostByUserNameQueryHandler(
    IPostRepository postRepository,
    IUserRepository userRepository)
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

        var posts = postRepository.GetPostsByUserName(userNameResult.Value, request.Offset, ct);

        List<UserPostItem> postsDetails = posts.ToUserPostItemResponse();

        var response = new UserPostResponse(new UserResponse(user), postsDetails);
        return response;
    }
}