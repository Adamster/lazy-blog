using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Comments.GetCommentById;
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

    public async Task<Result<List<CommentResponse>>> Handle(GetCommentByPostIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure<List<CommentResponse>>(DomainErrors.Post.NotFound(request.PostId));
        }

        var tmpAvatarUrl = "https://metro.co.uk/wp-content/uploads/2015/06/ad_174020392-e1487698550420.jpg";
        List<Comment> comments = await _commentRepository.GetAllAsync(post.Id, cancellationToken);

        List<CommentResponse> response = 
            comments.Select(
                    c => new CommentResponse
                    (
                        c.Id, 
                        c.User.UserName.Value, 
                        tmpAvatarUrl, 
                        c.CommentText.Value, 
                        c.CreatedOnUtc))
                .ToList();

        return response;
    }
}