using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.GetPostByUserId;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPostByUserName;

public record GetPostByUserNameQueryHandler : IQueryHandler<GetPostByUserNameQuery, List<UserPostResponse>>
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

    public async Task<Result<List<UserPostResponse>>> Handle(GetPostByUserNameQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.UserName, cancellationToken);

        if (user is null)
        {
            return Result.Failure<List<UserPostResponse>>(new Error(
                "User.NotFound",
                $"The user with Username {request.UserName} was not found."));
        }

        var posts = await _postRepository.GetPostsByUserNameAsync(request.UserName, request.Offset, cancellationToken);

        List<UserPostResponse> response = posts
            .Select(p =>
                new UserPostResponse(
                    p.Id,
                    p.Title.Value,
                    p.Summary.Value,
                    p.Slug.Value,
                    p.IsPublished,
                    p.CreatedOnUtc))
            .ToList();

        return response;
    }
}