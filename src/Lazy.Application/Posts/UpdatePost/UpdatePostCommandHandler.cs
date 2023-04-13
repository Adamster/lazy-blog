using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.UpdatePost;

public class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePostCommandHandler(
        IPostRepository postRepository, 
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);

        if (post is null)
        {
            return Result.Failure(DomainErrors.Post.NotFound(request.Id));
        }

        if (!_currentUserContext.IsCurrentUser(post.UserId))
        {
            return Result.Failure(DomainErrors.Post.UnauthorizedPostAccess);
        }

        Result<Title> titleResult = Title.Create(request.Title);
        Result<Summary> summaryResult = Summary.Create(request.Summary);
        Result<Body> bodyResult = Body.Create(request.Body);
        Result<Slug> slugResult = Slug.Create(request.Slug);

        post.Update(
            titleResult.Value,
            summaryResult.Value,
            bodyResult.Value,
            slugResult.Value,
            request.CoverUrl);

        _postRepository.Update(post);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}