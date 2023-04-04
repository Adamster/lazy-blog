using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Extensions;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.CreatePost;

internal sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, Guid>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public CreatePostCommandHandler(IPostRepository postRepository, IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        Result<Title> titleResult = Title.Create(request.Title);
        Result<Summary> summaryResult = Summary.Create(request.Summary);
        Result<Body> bodyResult = Body.Create(request.Body);
        Result<Slug> slugResult = Slug.Create(request.Title.Slugify());

        if (await _userRepository.GetByIdAsync(request.UserId, cancellationToken) is null)
        {
            return Result.Failure<Guid>(DomainErrors.User.NotFound(request.UserId));
        }

        if (await _postRepository.GetBySlugAsync(slugResult.Value, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(DomainErrors.Slug.SlugAlreadyInUse);
        }

        Post post = Post.Create(
            Guid.NewGuid(),
            titleResult.Value,
            summaryResult.Value,
            slugResult.Value,
            bodyResult.Value,
            request.UserId);

        _postRepository.Add(post);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}