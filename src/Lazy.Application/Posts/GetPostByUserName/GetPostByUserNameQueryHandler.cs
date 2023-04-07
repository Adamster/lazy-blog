using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Application.Users.GetUserById;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Posts.GetPostByUserName;

public class GetPostByUserNameQueryHandler : IQueryHandler<GetPostByUserNameQuery, UserPostResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;

    public GetPostByUserNameQueryHandler(
        IPostRepository postRepository, 
        IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<UserPostResponse>> Handle(GetPostByUserNameQuery request, CancellationToken cancellationToken)
    {
        var userNameResult = UserName.Create(request.UserName);
        var user = await _userRepository.GetByUsernameAsync(userNameResult.Value, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserPostResponse>(new Error(
                "User.NotFound",
                $"The user with Username {request.UserName} was not found."));
        }

        var posts = await _postRepository.GetPostsByUserNameAsync(userNameResult.Value, request.Offset, cancellationToken);

        List<UserPostDetails> postsDetails = posts
            .Select(p =>
                new UserPostDetails(
                    p.Id,
                    p.Title.Value,
                    p.Summary.Value,
                    p.Slug.Value,
                    p.IsPublished,
                    p.CreatedOnUtc))
            .ToList();

        var response = new UserPostResponse(new UserResponse(user), postsDetails);
        return response;
    }
}