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
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentCommandHandler(
        IUserRepository userRepository,
        IPostRepository postRepository,
        IUnitOfWork unitOfWork, 
        ICommentRepository commentRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _commentRepository = commentRepository;
    }

    public async Task<Result<Guid>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Guid>(DomainErrors.User.NotFound(request.UserId));
        }

        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return Result.Failure<Guid>(DomainErrors.Post.NotFound(request.PostId));
        }

        Result<Body> commentBody = Body.Create(request.CommentText);

        var comment = Comment.Create(post, user, commentBody.Value);
        _commentRepository.Add(comment);
        //post.AddComment(comment);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}