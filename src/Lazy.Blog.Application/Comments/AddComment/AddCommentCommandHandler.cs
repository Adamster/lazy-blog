using Lazy.Application.Abstractions.Authorization;
using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Comments.AddComment;

public class AddCommentCommandHandler : ICommandHandler<AddCommentCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentCommandHandler(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork, 
        ICommentRepository commentRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
        _commentRepository = commentRepository;
    }

    public async Task<Result<Guid>> Handle(AddCommentCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            return Result.Failure<Guid>(DomainErrors.User.NotFound(request.UserId));
        }

        if (!_currentUserContext.IsCurrentUser(request.UserId))
        {
            return Result.Failure<Guid>(DomainErrors.Comment.UnauthorizedCommentUpdate);
        }

        var post = await _postRepository.GetByIdAsync(request.PostId, ct);
        if (post is null)
        {
            return Result.Failure<Guid>(DomainErrors.Post.NotFound(request.PostId));
        }

        Result<Body> commentBody = Body.Create(request.Body);

        var comment = Comment.Create(post, user, commentBody.Value);
        _commentRepository.Add(comment);
        

        await _unitOfWork.SaveChangesAsync(ct);

        return comment.Id;
    }
}