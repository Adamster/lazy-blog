using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.Extensions;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPostByUserId;

public class GetPostByIdQueryHandler : IQueryHandler<GetPostByUserIdQuery, UserPostResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public GetPostByIdQueryHandler(
        IPostRepository postRepository,
        IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<UserPostResponse>> Handle(GetPostByUserIdQuery request, CancellationToken ct)
    {
        User? user = await _userRepository.GetByIdAsync(request.UserId, ct);

        if (user is null)
        {
            return Result.Failure<UserPostResponse>(new Error(
                "User.NotFound",
                $"The user with Id {request.UserId} was not found."));
        }

        var posts = _postRepository
            .GetPostsByUserId(request.UserId, request.Offset, ct);

        var postDetails = posts.ToUserPostItemResponse();

        int postCount = await _postRepository.GetPostCountByUserIdAsync(user.Id, ct);

        var response = new UserPostResponse(new UserResponse(user), postDetails, postCount);
        return response;
    }
}