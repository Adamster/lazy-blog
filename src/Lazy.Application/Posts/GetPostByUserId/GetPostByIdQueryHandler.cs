using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Posts.GetPostByUserId;

public class GetPostByIdQueryHandler : IQueryHandler<GetPostByUserIdQuery, List<UserPostResponse>>
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

    public async Task<Result<List<UserPostResponse>>> Handle(GetPostByUserIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<List<UserPostResponse>>(new Error(
                "User.NotFound",
                $"The user with Id {request.UserId} was not found."));
        }

        var posts = await _postRepository
           .GetPostsByUserIdAsync(request.UserId, request.Offset, cancellationToken);

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