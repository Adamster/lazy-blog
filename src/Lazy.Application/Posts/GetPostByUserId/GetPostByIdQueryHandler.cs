using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Posts.GetPublishedPosts;
using Lazy.Application.Users.GetUserById;
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

    public async Task<Result<UserPostResponse>> Handle(GetPostByUserIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserPostResponse>(new Error(
                "User.NotFound",
                $"The user with Id {request.UserId} was not found."));
        }

        var posts = await _postRepository
           .GetPostsByUserIdAsync(request.UserId, request.Offset, cancellationToken);

       var postDetails = posts
           .Select(p =>
               new UserPostItem(
                   p.Id,
                   p.Title.Value,
                   p.Summary.Value,
                   p.Slug.Value,
                   p.Views,
                   p.Comments.Count,
                   p.CoverUrl,
                   p.IsPublished,
                   p.CreatedOnUtc))
           .ToList();

       var response = new UserPostResponse(new UserResponse(user), postDetails);
       return response;
    }
}