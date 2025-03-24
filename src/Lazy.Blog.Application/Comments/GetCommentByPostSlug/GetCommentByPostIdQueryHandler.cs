using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Comments.GetCommentById;
using Lazy.Application.Users.Extensions;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Comments.GetCommentByPostSlug;

public class GetCommentByPostIdQueryHandler : IQueryHandler<GetCommentByPostIdQuery, List<CommentResponse>>
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;

    public GetCommentByPostIdQueryHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
    }

    public async Task<Result<List<CommentResponse>>> Handle(GetCommentByPostIdQuery request, CancellationToken ct)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, ct);

        if (post is null)
        {
            return Result.Failure<List<CommentResponse>>(DomainErrors.Post.NotFound(request.PostId));
        }

        List<Comment> comments = await _commentRepository.GetAllAsync(post.Id, ct);

        List<CommentResponse> response = comments.Select(c
                => new CommentResponse(
                        c.Id,
                        c.User.ToUserCommentResponse(),
                        c.CommentText.Value,
                        c.CreatedOnUtc))
            .ToList();

        return response;
    }
}