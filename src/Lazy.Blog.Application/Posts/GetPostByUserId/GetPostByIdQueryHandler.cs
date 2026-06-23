using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.Shared;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPostByUserId;

public class GetPostByIdQueryHandler(
    IPostRepository postRepository,
    IUserRepository userRepository,
    IUserPostResponseBuilder userPostResponseBuilder)
    : IQueryHandler<GetPostByUserIdQuery, UserPostResponse>
{
    public async Task<Result<UserPostResponse>> Handle(GetPostByUserIdQuery request, CancellationToken ct)
    {
        User? user = await userRepository.GetByIdAsync(request.UserId, ct);

        if (user is null)
        {
            return Result.Failure<UserPostResponse>(new Error(
                "User.NotFound",
                $"The user with Id {request.UserId} was not found."));
        }

        var posts = postRepository.GetPostsByUserId(request.UserId, request.Offset, ct);

        return await userPostResponseBuilder.BuildAsync(user, posts, ct);
    }
}
